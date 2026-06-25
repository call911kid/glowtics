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
    public record UpdateProductAvailabilityCommand(
        Guid ProductId, 
        Guid RetailerId, 
        bool IsAvailable
    ) : IRequest;

    public class UpdateProductAvailabilityCommandHandler : IRequestHandler<UpdateProductAvailabilityCommand>
    {
        private readonly GlowticsDbContext _dbContext;

        public UpdateProductAvailabilityCommandHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(UpdateProductAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.RetailerId == request.RetailerId, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.ProductNotFound, $"Entity 'Product' ({request.ProductId}) was not found.");

            product.IsAvailable = request.IsAvailable;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

