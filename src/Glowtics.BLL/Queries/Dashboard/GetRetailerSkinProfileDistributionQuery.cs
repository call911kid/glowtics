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
    public record GetRetailerSkinProfileDistributionQuery(Guid UserId) : IRequest<GetRetailerSkinProfileDistributionResponse>;

    public record SkinProfileCountDto(string Profile, int Count);

    public record GetRetailerSkinProfileDistributionResponse(List<SkinProfileCountDto> Distribution);

    public class GetRetailerSkinProfileDistributionQueryHandler : IRequestHandler<GetRetailerSkinProfileDistributionQuery, GetRetailerSkinProfileDistributionResponse>
    {
        private readonly GlowticsDbContext _context;

        public GetRetailerSkinProfileDistributionQueryHandler(GlowticsDbContext context)
        {
            _context = context;
        }

        public async Task<GetRetailerSkinProfileDistributionResponse> Handle(GetRetailerSkinProfileDistributionQuery request, CancellationToken cancellationToken)
        {
            var retailerId = await _context.Retailers
                .Where(r => r.UserId == request.UserId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (retailerId == Guid.Empty)
            {
                throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"No active retailer found for user id '{request.UserId}'.");
            }

            var distribution = await _context.DiagnosticSessions
                .Where(ds => ds.RetailerId == retailerId && !string.IsNullOrEmpty(ds.SkinProfileResult))
                .GroupBy(ds => ds.SkinProfileResult)
                .Select(g => new SkinProfileCountDto(
                    g.Key,
                    g.Count()
                ))
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);

            return new GetRetailerSkinProfileDistributionResponse(distribution);
        }
    }
}
