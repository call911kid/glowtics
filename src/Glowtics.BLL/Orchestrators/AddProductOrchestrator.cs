using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Glowtics.BLL.Commands.Products;
using Glowtics.BLL.Commands.Embeddings;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Orchestrators
{
    public record AddProductOrchestratorRequest(
        Guid RetailerId, 
        string ExternalProductId, 
        string Name, 
        List<string> TargetConditions,
        List<string> ActiveIngredients,
        List<string> Conflicts,
        List<string> ImageUrls
    ) : IRequest<AddProductResponse>;

    // The Response
    public class AddProductResponse
    {
        public Guid ProductId { get; set; }
    }

    public class AddProductOrchestrator : IRequestHandler<AddProductOrchestratorRequest, AddProductResponse>
    {
        private readonly IMediator _mediator;
        private readonly GlowticsDbContext _dbContext;

        public AddProductOrchestrator(IMediator mediator, GlowticsDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        public async Task<AddProductResponse> Handle(AddProductOrchestratorRequest request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Id == request.RetailerId, cancellationToken)
                ?? throw new RetailerNotFoundException($"Retailer ({request.RetailerId}) was not found.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var addProductCommand = new AddProductCommand(
                    retailer.Id,
                    request.ExternalProductId,
                    request.Name,
                    request.TargetConditions,
                    request.ActiveIngredients,
                    request.Conflicts,
                    request.ImageUrls
                );

                var productId = await _mediator.Send(addProductCommand, cancellationToken);

                var embeddingCommand = new AddEmbeddingCommand(
                    retailer.MongoCollectionName,
                    request.ExternalProductId,
                    request.Name,
                    request.TargetConditions,
                    request.ActiveIngredients,
                    request.Conflicts
                );
                
                await _mediator.Send(embeddingCommand, cancellationToken);

                // 3. Commit the transaction
                await transaction.CommitAsync(cancellationToken);

                return new AddProductResponse { ProductId = productId };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

