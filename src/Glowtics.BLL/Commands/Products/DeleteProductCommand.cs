using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Commands.Products
{
    public record DeleteProductCommand(Guid ProductId, Guid RetailerId) : IRequest;

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly GlowticsDbContext _dbContext;

        public DeleteProductCommandHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.RetailerId == request.RetailerId, cancellationToken);

            if (product == null)
            {
                throw new EntityNotFoundException(ErrorCodes.ProductNotFound, $"Entity 'Product' ({request.ProductId}) was not found.");
            }

            product.IsDeleted = true;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

