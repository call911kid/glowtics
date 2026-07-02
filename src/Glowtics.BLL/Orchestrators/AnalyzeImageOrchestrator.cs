using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Orchestrators
{
    /// <summary>Step 1 of the two-step analyze: validate + analyze the photo into a skin profile.
    /// If the SAME user submitted the SAME image before, return that cached result and skip the AI.</summary>
    public record AnalyzeImageOrchestratorRequest(byte[] PhotoBytes, string FileName, string ContentType, string Domain, string? ExternalUserId)
        : IRequest<AnalyzeImageResponse>;

    public class AnalyzeImageResponse
    {
        public bool Accepted { get; set; }
        public string? Message { get; set; }

        /// <summary>True when a prior identical (user + image) analysis was found — Products/Session are the old ones.</summary>
        public bool Cached { get; set; }

        /// <summary>SHA-256 of the photo — the client sends it back to step 2 so the routine ties to this image.</summary>
        public string ImageHash { get; set; } = string.Empty;

        /// <summary>The skin analysis / diagnosis (JSON) from the analysis flow.</summary>
        public string SkinProfile { get; set; } = string.Empty;

        // Populated only when Cached == true (the previously built routine).
        public Guid? SessionId { get; set; }
        public List<RecommendedProductDto> Products { get; set; } = new();
        public string? CartRedirectUrl { get; set; }
    }

    public class AnalyzeImageOrchestrator : IRequestHandler<AnalyzeImageOrchestratorRequest, AnalyzeImageResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly IAdvancedLangflowService _langflowService;
        private readonly IAnalysisTracer _tracer;

        public AnalyzeImageOrchestrator(GlowticsDbContext dbContext, IAdvancedLangflowService langflowService, IAnalysisTracer tracer)
        {
            _dbContext = dbContext;
            _langflowService = langflowService;
            _tracer = tracer;
        }

        public async Task<AnalyzeImageResponse> Handle(AnalyzeImageOrchestratorRequest request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Domain == request.Domain, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"Retailer with domain '{request.Domain}' was not found.");

            var imageHash = Convert.ToHexString(SHA256.HashData(request.PhotoBytes)).ToLowerInvariant();

            // Dedup: same user + same image already analyzed -> return the old result directly.
            if (!string.IsNullOrWhiteSpace(request.ExternalUserId))
            {
                var prior = await _dbContext.DiagnosticSessions
                    .Include(s => s.RecommendedProducts)
                    .Where(s => s.RetailerId == retailer.Id
                                && s.ExternalUserId == request.ExternalUserId
                                && s.ImageHash == imageHash)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (prior != null)
                {
                    // Replay the real per-product rationale from the saved routine JSON (fall back to a generic label).
                    var rationaleByExtId = BuildRationaleLookup(prior.RoutineJson, prior.RecommendedProducts);

                    return new AnalyzeImageResponse
                    {
                        Accepted = true,
                        Cached = true,
                        ImageHash = imageHash,
                        // Prefer the full saved profile JSON; fall back to the short badge for old sessions.
                        SkinProfile = string.IsNullOrWhiteSpace(prior.SkinProfileJson) ? prior.SkinProfileResult : prior.SkinProfileJson!,
                        SessionId = prior.Id,
                        CartRedirectUrl = retailer.CartRedirectUrl,
                        Products = prior.RecommendedProducts.Select(p => new RecommendedProductDto
                        {
                            Rationale = rationaleByExtId.TryGetValue(p.ExternalProductId, out var r) ? r : "Recommended by AI",
                            ExternalProductId = p.ExternalProductId,
                            Name = p.Name,
                            IsAvailable = p.IsAvailable,
                            ActiveIngredients = p.ActiveIngredients,
                            ImageUrls = p.ImageUrls
                        }).ToList()
                    };
                }
            }

            // 1. VALIDATION flow.
            var sw = Stopwatch.StartNew();
            var validation = await _langflowService.ValidateAsync(request.PhotoBytes, request.FileName, cancellationToken);
            sw.Stop();
            await _tracer.TraceAsync(new AnalysisTrace
            {
                Name = "validate",
                LatencyMs = sw.ElapsedMilliseconds,
                Accepted = validation.IsValid,
                Collection = retailer.MongoCollectionName,
                Model = "glowtics-validation-flow"
            }, cancellationToken);

            if (!validation.IsValid)
            {
                return new AnalyzeImageResponse { Accepted = false, Message = validation.Reason, ImageHash = imageHash };
            }

            // 2. ANALYSIS flow -> skin profile (no routine yet; that's step 2).
            sw.Restart();
            var skinProfile = await _langflowService.AnalyzeSkinProfileAsync(
                request.PhotoBytes, request.FileName, retailer.MongoCollectionName, cancellationToken);
            sw.Stop();
            await _tracer.TraceAsync(new AnalysisTrace
            {
                Name = "analyze",
                LatencyMs = sw.ElapsedMilliseconds,
                Accepted = true,
                Collection = retailer.MongoCollectionName,
                Model = "glowtics-analysis-flow"
            }, cancellationToken);

            return new AnalyzeImageResponse
            {
                Accepted = true,
                Cached = false,
                ImageHash = imageHash,
                SkinProfile = skinProfile
            };
        }

        /// <summary>Map each recommended product's ExternalProductId -> its saved rationale, parsed from the routine JSON.
        /// The routine's productId may be a Mongo _id, so match by id first, then by product name.</summary>
        private static Dictionary<string, string> BuildRationaleLookup(
            string? routineJson, IEnumerable<DAL.Entities.Product> products)
        {
            var map = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(routineJson)) return map;

            List<DTOs.LangflowRoutineItem> items;
            try
            {
                var parsed = System.Text.Json.JsonSerializer.Deserialize<DTOs.LangflowRoutineResponse>(
                    routineJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                items = parsed?.Routine ?? new List<DTOs.LangflowRoutineItem>();
            }
            catch (System.Text.Json.JsonException) { return map; }

            string Norm(string s) => System.Text.RegularExpressions.Regex.Replace((s ?? string.Empty).Trim().ToLowerInvariant(), @"\s+", " ");

            foreach (var p in products)
            {
                var item = items.FirstOrDefault(i =>
                    string.Equals(i.ProductId?.Trim(), p.ExternalProductId, StringComparison.OrdinalIgnoreCase))
                    ?? items.FirstOrDefault(i => Norm(i.ProductName) == Norm(p.Name));

                if (item != null && !string.IsNullOrWhiteSpace(item.Rationale))
                {
                    map[p.ExternalProductId] = item.Rationale;
                }
            }
            return map;
        }
    }
}
