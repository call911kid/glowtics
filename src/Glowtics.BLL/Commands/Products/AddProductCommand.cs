using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using MediatR;

namespace Glowtics.BLL.Commands.Products
{
    public record AddProductCommand(
        Guid RetailerId, 
        string ExternalProductId, 
        string Name, 
        List<string> TargetConditions,
        List<string> ActiveIngredients,
        List<string> Conflicts,
        List<string> ImageUrls
    ) : IRequest<Guid>;

    public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Guid>
    {
        private readonly GlowticsDbContext _dbContext;

        public AddProductCommandHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                RetailerId = request.RetailerId,
                ExternalProductId = request.ExternalProductId,
                Name = request.Name,
                TargetConditions = request.TargetConditions,
                ActiveIngredients = request.ActiveIngredients,
                Conflicts = request.Conflicts,
                ImageUrls = request.ImageUrls,
                IsAvailable = true,
                IsDeleted = false
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
