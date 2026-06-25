using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Glowtics.BLL.Commands.Products;
using Glowtics.BLL.Commands.Embeddings;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Orchestrators
{
    public record UpdateProductAvailabilityOrchestratorRequest(
        Guid RetailerId, 
        Guid ProductId,
        bool IsAvailable
    ) : IRequest<UpdateProductAvailabilityResponse>;

    public class UpdateProductAvailabilityResponse
    {
        public bool Success { get; set; }
    }

    public class UpdateProductAvailabilityOrchestrator : IRequestHandler<UpdateProductAvailabilityOrchestratorRequest, UpdateProductAvailabilityResponse>
    {
        private readonly IMediator _mediator;
        private readonly GlowticsDbContext _dbContext;

        public UpdateProductAvailabilityOrchestrator(IMediator mediator, GlowticsDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        public async Task<UpdateProductAvailabilityResponse> Handle(UpdateProductAvailabilityOrchestratorRequest request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Id == request.RetailerId, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"Entity 'Retailer' ({request.RetailerId}) was not found.");

            var product = await _dbContext.Products
                .Where(p => p.Id == request.ProductId && p.RetailerId == retailer.Id)
                .Select(p => new { p.ExternalProductId, p.IsAvailable })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.ProductNotFound, $"Entity 'Product' ({request.ProductId}) was not found.");

            // If the requested state matches the current state, exit early
            if (product.IsAvailable == request.IsAvailable)
            {
                return new UpdateProductAvailabilityResponse { Success = true };
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Update SQL Server entity
                await _mediator.Send(new UpdateProductAvailabilityCommand(
                    request.ProductId, 
                    retailer.Id, 
                    request.IsAvailable
                ), cancellationToken);

                // 2. Update MongoDB vector document
                await _mediator.Send(new UpdateEmbeddingAvailabilityCommand(
                    retailer.MongoCollectionName, 
                    product.ExternalProductId, 
                    request.IsAvailable
                ), cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new UpdateProductAvailabilityResponse { Success = true };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

