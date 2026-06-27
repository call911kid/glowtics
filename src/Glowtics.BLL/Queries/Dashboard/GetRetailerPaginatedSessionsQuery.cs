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
    public record GetRetailerPaginatedSessionsQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) : IRequest<GetRetailerPaginatedSessionsResponse>;

    public record DiagnosticSessionDto(Guid Id, string SkinProfileResult, DateTime CreatedAt);

    public record GetRetailerPaginatedSessionsResponse(List<DiagnosticSessionDto> Sessions, int TotalCount, int PageNumber, int PageSize);

    public class GetRetailerPaginatedSessionsQueryHandler : IRequestHandler<GetRetailerPaginatedSessionsQuery, GetRetailerPaginatedSessionsResponse>
    {
        private readonly GlowticsDbContext _context;

        public GetRetailerPaginatedSessionsQueryHandler(GlowticsDbContext context)
        {
            _context = context;
        }

        public async Task<GetRetailerPaginatedSessionsResponse> Handle(GetRetailerPaginatedSessionsQuery request, CancellationToken cancellationToken)
        {
            var retailerId = await _context.Retailers
                .Where(r => r.UserId == request.UserId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (retailerId == Guid.Empty)
            {
                throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"No active retailer found for user id '{request.UserId}'.");
            }

            var query = _context.DiagnosticSessions
                .Where(ds => ds.RetailerId == retailerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var sessions = await query
                .OrderByDescending(ds => ds.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ds => new DiagnosticSessionDto(
                    ds.Id,
                    ds.SkinProfileResult,
                    ds.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return new GetRetailerPaginatedSessionsResponse(sessions, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
