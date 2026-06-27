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
    public record GetRetailerTopProductsQuery(Guid UserId, int Limit = 5) : IRequest<GetRetailerTopProductsResponse>;

    public record ProductCountDto(Guid ProductId, string ProductName, int RecommendationCount);

    public record GetRetailerTopProductsResponse(List<ProductCountDto> TopProducts);

    public class GetRetailerTopProductsQueryHandler : IRequestHandler<GetRetailerTopProductsQuery, GetRetailerTopProductsResponse>
    {
        private readonly GlowticsDbContext _context;

        public GetRetailerTopProductsQueryHandler(GlowticsDbContext context)
        {
            _context = context;
        }

        public async Task<GetRetailerTopProductsResponse> Handle(GetRetailerTopProductsQuery request, CancellationToken cancellationToken)
        {
            var retailerId = await _context.Retailers
                .Where(r => r.UserId == request.UserId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (retailerId == Guid.Empty)
            {
                throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"No active retailer found for user id '{request.UserId}'.");
            }

            var topProducts = await _context.DiagnosticSessions
                .Where(ds => ds.RetailerId == retailerId)
                .SelectMany(ds => ds.RecommendedProducts)
                .GroupBy(p => new { p.Id, p.Name })
                .Select(g => new ProductCountDto(
                    g.Key.Id,
                    g.Key.Name,
                    g.Count()
                ))
                .OrderByDescending(x => x.RecommendationCount)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            return new GetRetailerTopProductsResponse(topProducts);
        }
    }
}
