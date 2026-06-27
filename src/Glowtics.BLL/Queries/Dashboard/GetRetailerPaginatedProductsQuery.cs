using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Glowtics.BLL.Queries.Dashboard
{
    public record GetRetailerPaginatedProductsQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) : IRequest<GetRetailerPaginatedProductsResponse>;

    public record ProductDto(Guid Id, string Name, bool IsAvailable);

    public record GetRetailerPaginatedProductsResponse(List<ProductDto> Products, int TotalCount, int PageNumber, int PageSize);

    public class GetRetailerPaginatedProductsQueryHandler : IRequestHandler<GetRetailerPaginatedProductsQuery, GetRetailerPaginatedProductsResponse>
    {
        private readonly GlowticsDbContext _context;

        public GetRetailerPaginatedProductsQueryHandler(GlowticsDbContext context)
        {
            _context = context;
        }

        public async Task<GetRetailerPaginatedProductsResponse> Handle(GetRetailerPaginatedProductsQuery request, CancellationToken cancellationToken)
        {
            var retailerId = await _context.Retailers
                .Where(r => r.UserId == request.UserId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (retailerId == Guid.Empty)
            {
                throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"No active retailer found for user id '{request.UserId}'.");
            }

            var query = _context.Products
                .Where(p => p.RetailerId == retailerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.IsAvailable
                ))
                .ToListAsync(cancellationToken);

            return new GetRetailerPaginatedProductsResponse(products, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
