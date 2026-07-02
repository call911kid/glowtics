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

    public record SessionProductDto(string Name, string ImageUrl);
    public record DiagnosticSessionDto(Guid Id, string SkinProfileResult, string? SkinProfileJson, string? RoutineJson, string? ExternalUserId, string? Feedback, List<SessionProductDto> RecommendedProducts, DateTime CreatedAt);

    public record GetRetailerPaginatedSessionsResponse(List<DiagnosticSessionDto> Sessions, int TotalCount, int UniqueUsersCount, int PageNumber, int PageSize);

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

            var query = _context.DiagnosticSessions.Where(ds => ds.RetailerId == retailerId);

            var totalCount = await query.CountAsync(cancellationToken);
            var uniqueUsersCount = await query
                .Where(s => s.ExternalUserId != null)
                .Select(s => s.ExternalUserId)
                .Distinct()
                .CountAsync(cancellationToken);

            // Materialize the page, then map in memory (ImageUrls is a value-converted list — can't project its
            // first element in SQL).
            var page = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new
                {
                    s.Id,
                    s.SkinProfileResult,
                    s.SkinProfileJson,
                    s.RoutineJson,
                    s.ExternalUserId,
                    s.Feedback,
                    s.CreatedAt,
                    Products = s.RecommendedProducts.Select(p => new { p.Name, p.ImageUrls }).ToList()
                })
                .ToListAsync(cancellationToken);

            var sessions = page.Select(s => new DiagnosticSessionDto(
                s.Id,
                s.SkinProfileResult,
                s.SkinProfileJson,
                s.RoutineJson,
                s.ExternalUserId,
                s.Feedback,
                s.Products.Select(p => new SessionProductDto(
                    p.Name,
                    p.ImageUrls != null && p.ImageUrls.Count > 0 ? p.ImageUrls[0] : string.Empty)).ToList(),
                s.CreatedAt)).ToList();

            return new GetRetailerPaginatedSessionsResponse(sessions, totalCount, uniqueUsersCount, request.PageNumber, request.PageSize);
        }
    }
}
