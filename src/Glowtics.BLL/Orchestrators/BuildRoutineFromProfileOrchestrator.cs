using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Commands.Diagnostics;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Orchestrators
{
    /// <summary>Step 2 of the two-step analyze: skin profile -> RAG + rerank -> product routine, then persist the session.</summary>
    public record BuildRoutineFromProfileOrchestratorRequest(string SkinProfile, string? ImageHash, string Domain, string? ExternalUserId)
        : IRequest<BuildRoutineFromProfileResponse>;

    public class BuildRoutineFromProfileResponse
    {
        public Guid? SessionId { get; set; }
        public string SkinProfileResult { get; set; } = string.Empty;
        public List<RecommendedProductDto> Products { get; set; } = new();
        public string? CartRedirectUrl { get; set; }
    }

    public class BuildRoutineFromProfileOrchestrator
        : IRequestHandler<BuildRoutineFromProfileOrchestratorRequest, BuildRoutineFromProfileResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly IAdvancedLangflowService _langflowService;
        private readonly IAnalysisTracer _tracer;
        private readonly IMediator _mediator;

        public BuildRoutineFromProfileOrchestrator(
            GlowticsDbContext dbContext, IAdvancedLangflowService langflowService, IAnalysisTracer tracer, IMediator mediator)
        {
            _dbContext = dbContext;
            _langflowService = langflowService;
            _tracer = tracer;
            _mediator = mediator;
        }

        public async Task<BuildRoutineFromProfileResponse> Handle(BuildRoutineFromProfileOrchestratorRequest request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Domain == request.Domain, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"Retailer with domain '{request.Domain}' was not found.");

            // BUILD ROUTINE flow — skin profile -> RAG + rerank -> routine + product ids.
            var sw = Stopwatch.StartNew();
            var diagnosis = await _langflowService.BuildRoutineAsync(request.SkinProfile, retailer.MongoCollectionName, cancellationToken);
            sw.Stop();

            var dbProducts = await _dbContext.Products
                .Where(p => p.RetailerId == retailer.Id && diagnosis.ExternalProductIds.Contains(p.ExternalProductId))
                .ToListAsync(cancellationToken);

            var validProductIds = dbProducts.Select(p => p.ExternalProductId).ToList();

            await _tracer.TraceAsync(new AnalysisTrace
            {
                Name = "build-routine",
                LatencyMs = sw.ElapsedMilliseconds,
                Accepted = true,
                ProductCount = validProductIds.Count,
                Collection = retailer.MongoCollectionName,
                Model = "glowtics-build-routine-flow"
            }, cancellationToken);

            // Persist the short skin TYPE (from the step-1 profile) — the dashboard shows this as a badge,
            // so it must not be the full routine/analysis JSON blob.
            var skinType = ExtractSkinType(request.SkinProfile);
            var addSessionResult = await _mediator.Send(new AddDiagnosticSessionCommand(
                retailer.Id, skinType, validProductIds, request.ExternalUserId, request.ImageHash), cancellationToken);

            var recommendedProducts = new List<RecommendedProductDto>();
            foreach (var aiRecId in diagnosis.ExternalProductIds) // keep AI order
            {
                var dbProd = dbProducts.FirstOrDefault(p => p.ExternalProductId == aiRecId);
                if (dbProd != null && !recommendedProducts.Any(p => p.ExternalProductId == dbProd.ExternalProductId))
                {
                    // Prefer the AI's per-product rationale; fall back to a generic label.
                    var rationale = diagnosis.RoutineItems
                        .FirstOrDefault(i => i.ProductId == aiRecId)?.Rationale;

                    recommendedProducts.Add(new RecommendedProductDto
                    {
                        Rationale = string.IsNullOrWhiteSpace(rationale) ? "Recommended by AI" : rationale!,
                        ExternalProductId = dbProd.ExternalProductId,
                        Name = dbProd.Name,
                        IsAvailable = dbProd.IsAvailable,
                        ActiveIngredients = dbProd.ActiveIngredients,
                        ImageUrls = dbProd.ImageUrls
                    });
                }
            }

            return new BuildRoutineFromProfileResponse
            {
                SessionId = addSessionResult.SessionId,
                SkinProfileResult = diagnosis.SkinProfileResult,
                Products = recommendedProducts,
                CartRedirectUrl = retailer.CartRedirectUrl
            };
        }

        /// <summary>Pull the short overallSkinType out of the step-1 analysis JSON (fallback "Analyzed").</summary>
        private static string ExtractSkinType(string skinProfile)
        {
            if (string.IsNullOrWhiteSpace(skinProfile)) return "Unknown";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(skinProfile);
                if (doc.RootElement.TryGetProperty("overallSkinType", out var t) && t.GetString() is { Length: > 0 } s)
                {
                    return s;
                }
            }
            catch (System.Text.Json.JsonException) { /* non-JSON profile */ }
            var trimmed = skinProfile.Trim();
            return trimmed.Length <= 40 && !trimmed.StartsWith("{") ? trimmed : "Analyzed";
        }
    }
}
