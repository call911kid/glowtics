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
    public record AdvancedAnalyzeOrchestratorRequest(byte[] PhotoBytes, string FileName, string ContentType, string Domain, string? ExternalUserId) : IRequest<AdvancedAnalyzeResponse>;

    public class AdvancedAnalyzeResponse
    {
        public bool Accepted { get; set; }
        public string? Message { get; set; }
        public Guid? SessionId { get; set; }
        public string SkinProfileResult { get; set; } = string.Empty;

        public List<RecommendedProductDto> Products { get; set; } = new();
        public string? CartRedirectUrl { get; set; }
    }

    public class AdvancedAnalyzeOrchestrator : IRequestHandler<AdvancedAnalyzeOrchestratorRequest, AdvancedAnalyzeResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly IAdvancedLangflowService _langflowService;
        private readonly IAnalysisTracer _tracer;
        private readonly IMediator _mediator;

        public AdvancedAnalyzeOrchestrator(
            GlowticsDbContext dbContext,
            IAdvancedLangflowService langflowService,
            IAnalysisTracer tracer,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _langflowService = langflowService;
            _tracer = tracer;
            _mediator = mediator;
        }

        public async Task<AdvancedAnalyzeResponse> Handle(AdvancedAnalyzeOrchestratorRequest request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Domain == request.Domain, cancellationToken)
                ?? throw new RetailerNotFoundException($"Retailer with domain '{request.Domain}' was not found.");

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
                return new AdvancedAnalyzeResponse
                {
                    Accepted = false,
                    Message = validation.Reason
                };
            }

            sw.Restart();
            var diagnosis = await _langflowService.AnalyzeRoutineAsync(
                request.PhotoBytes, request.FileName, retailer.MongoCollectionName, cancellationToken);
            sw.Stop();
            
            var dbProducts = await _dbContext.Products
                .Where(p => p.RetailerId == retailer.Id && diagnosis.ExternalProductIds.Contains(p.ExternalProductId))
                .ToListAsync(cancellationToken);

            var validProductIds = dbProducts.Select(p => p.ExternalProductId).ToList();

            await _tracer.TraceAsync(new AnalysisTrace
            {
                Name = "analyze",
                LatencyMs = sw.ElapsedMilliseconds,
                Accepted = true,
                ProductCount = validProductIds.Count,
                Collection = retailer.MongoCollectionName,
                Model = "glowtics-analysis-flow"
            }, cancellationToken);

            var command = new AddDiagnosticSessionCommand(
                retailer.Id,
                diagnosis.SkinProfileResult,
                validProductIds,
                request.ExternalUserId
            );

            var addSessionResult = await _mediator.Send(command, cancellationToken);

            var recommendedProducts = new List<RecommendedProductDto>();
            foreach (var aiRecId in diagnosis.ExternalProductIds) // Keep the AI order if possible, or just add matches
            {
                var dbProd = dbProducts.FirstOrDefault(p => p.ExternalProductId == aiRecId);
                if (dbProd != null && !recommendedProducts.Any(p => p.ExternalProductId == dbProd.ExternalProductId))
                {
                    recommendedProducts.Add(new RecommendedProductDto
                    {
                        Rationale = "Recommended by AI", 
                        ExternalProductId = dbProd.ExternalProductId,
                        Name = dbProd.Name,
                        IsAvailable = dbProd.IsAvailable,
                        ActiveIngredients = dbProd.ActiveIngredients,
                        ImageUrls = dbProd.ImageUrls
                    });
                }
            }

            return new AdvancedAnalyzeResponse
            {
                Accepted = true,
                Message = "Success",
                SessionId = addSessionResult.SessionId,
                SkinProfileResult = diagnosis.SkinProfileResult,
                Products = recommendedProducts,
                CartRedirectUrl = retailer.CartRedirectUrl
            };
        }
    }
}
