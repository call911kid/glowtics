using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Commands.Diagnostics;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Glowtics.BLL.Orchestrators
{
    public record AnalyzeOrchestratorRequest(byte[] PhotoBytes, string FileName, string ContentType, string Domain) : IRequest<AnalyzeResponse>;

    public class AnalyzeResponse
    {
        public List<RecommendedProductDto> Products { get; set; } = new();
        public string? CartRedirectUrl { get; set; }
    }

    public class RecommendedProductDto
    {
        public string Rationale { get; set; } = string.Empty;

        public string ExternalProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public List<string> ActiveIngredients { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
    }

    public class AnalyzeOrchestrator : IRequestHandler<AnalyzeOrchestratorRequest, AnalyzeResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly ILangflowService _langflowService;
        private readonly IMediator _mediator;

        public AnalyzeOrchestrator(
            GlowticsDbContext dbContext,
            ILangflowService langflowService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _langflowService = langflowService;
            _mediator = mediator;
        }

        public async Task<AnalyzeResponse> Handle(AnalyzeOrchestratorRequest request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Domain == request.Domain, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"Retailer with domain '{request.Domain}' was not found.");
            var diagnosisResult = await _langflowService.DiagnoseAsync(
                request.PhotoBytes, 
                request.FileName, 
                request.ContentType,
                retailer.MongoCollectionName, 
                cancellationToken);

            var dbProducts = await _dbContext.Products
                .Where(p => p.RetailerId == retailer.Id && diagnosisResult.ExternalProductIds.Contains(p.ExternalProductId))
                .ToListAsync(cancellationToken);

            var validProductIds = dbProducts.Select(p => p.ExternalProductId).ToList();

            var command = new AddDiagnosticSessionCommand(
                retailer.Id,
                diagnosisResult.SkinProfileResult,
                validProductIds
            );

            var addSessionResult = await _mediator.Send(command, cancellationToken);

            var recommendedProducts = new List<RecommendedProductDto>();
            foreach (var aiRec in diagnosisResult.RoutineItems)
            {
                var dbProd = dbProducts.FirstOrDefault(p => p.ExternalProductId == aiRec.ProductId);
                if (dbProd != null)
                {
                    recommendedProducts.Add(new RecommendedProductDto
                    {
                        Rationale = aiRec.Rationale,
                        ExternalProductId = dbProd.ExternalProductId,
                        Name = dbProd.Name,
                        IsAvailable = dbProd.IsAvailable,
                        ActiveIngredients = dbProd.ActiveIngredients,
                        ImageUrls = dbProd.ImageUrls
                    });
                }
            }

            return new AnalyzeResponse
            {
                Products = recommendedProducts,
                CartRedirectUrl = retailer.CartRedirectUrl
            };
        }
    }
}
