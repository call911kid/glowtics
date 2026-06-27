using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Glowtics.BLL.Commands.Embeddings;
using Glowtics.BLL.Commands.Products;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Orchestrators
{
    public record DeleteProductOrchestratorRequest(
        Guid RetailerId, 
        string ExternalProductId
    ) : IRequest<DeleteProductResponse>;

    public class DeleteProductResponse
    {
        public bool Success { get; set; }
    }

    public class DeleteProductOrchestrator : IRequestHandler<DeleteProductOrchestratorRequest, DeleteProductResponse>
    {
        private readonly IMediator _mediator;
        private readonly GlowticsDbContext _dbContext;

        public DeleteProductOrchestrator(IMediator mediator, GlowticsDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        public async Task<DeleteProductResponse> Handle(DeleteProductOrchestratorRequest request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Id == request.RetailerId, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"Entity 'Retailer' ({request.RetailerId}) was not found.");

            var product = await _dbContext.Products
                .Where(p => p.ExternalProductId == request.ExternalProductId && p.RetailerId == retailer.Id)
                .Select(p => new { p.Id, p.ExternalProductId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.ProductNotFound, "No Product found with the given Id.");


            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _mediator.Send(new DeleteProductCommand(product.Id, retailer.Id), cancellationToken);

                await _mediator.Send(new DeleteEmbeddingCommand(retailer.MongoCollectionName, product.ExternalProductId), cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new DeleteProductResponse { Success = true };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

