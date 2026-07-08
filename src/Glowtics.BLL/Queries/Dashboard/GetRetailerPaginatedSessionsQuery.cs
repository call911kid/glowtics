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
    public record DiagnosticSessionDto(Guid Id, string SkinProfileResult, string? ExternalUserId, string? Feedback, List<SessionProductDto> RecommendedProducts, DateTime CreatedAt);

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
                throw new RetailerNotFoundException($"No active retailer found for user id '{request.UserId}'.");
            }

            // --- MOCK DATA ---
            var mockProducts1 = new List<SessionProductDto>
            {
                new SessionProductDto("Hydrating Cleanser", "https://glowtics.com/images/cleanser.jpg"),
                new SessionProductDto("Vitamin C Serum", "https://glowtics.com/images/serum.jpg")
            };

            var mockProducts2 = new List<SessionProductDto>
            {
                new SessionProductDto("Oil-Free Moisturizer", "https://glowtics.com/images/moisturizer.jpg")
            };

            var allMockSessions = new List<DiagnosticSessionDto>
            {
                new DiagnosticSessionDto(Guid.NewGuid(), "Oily, Acne-Prone", "user_123", "Loved the product recommendations, my skin feels much less oily after a week!", mockProducts2, DateTime.UtcNow.AddMinutes(-5)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Dry, Sensitive", "user_456", null, mockProducts1, DateTime.UtcNow.AddHours(-1)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Combination", "user_789", "The cleanser was a bit too harsh.", mockProducts1, DateTime.UtcNow.AddHours(-3)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Normal, Aging", "user_123", null, mockProducts1, DateTime.UtcNow.AddDays(-1)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Oily", "user_321", "Great results!", mockProducts2, DateTime.UtcNow.AddDays(-1).AddHours(-4)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Dry", "user_555", null, new List<SessionProductDto>(), DateTime.UtcNow.AddDays(-2)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Combination, Sensitive", "user_789", null, mockProducts1, DateTime.UtcNow.AddDays(-3)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Dry, Aging", "user_123", "Loved it.", mockProducts1, DateTime.UtcNow.AddDays(-4)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Oily, Sensitive", "user_321", null, mockProducts2, DateTime.UtcNow.AddDays(-5)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Normal", "user_999", null, new List<SessionProductDto>(), DateTime.UtcNow.AddDays(-6)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Combination", "user_111", "Okay.", mockProducts1, DateTime.UtcNow.AddDays(-7)),
                new DiagnosticSessionDto(Guid.NewGuid(), "Oily", "user_123", null, mockProducts2, DateTime.UtcNow.AddDays(-8))
            };

            // var query = _context.DiagnosticSessions.Where(ds => ds.RetailerId == retailerId);
            // var uniqueUsersCount = await query
            //     .Where(s => s.ExternalUserId != null)
            //     .Select(s => s.ExternalUserId)
            //     .Distinct()
            //     .CountAsync(cancellationToken);

            var totalCount = allMockSessions.Count;
            var uniqueUsersCount = allMockSessions
                .Where(s => !string.IsNullOrEmpty(s.ExternalUserId))
                .Select(s => s.ExternalUserId)
                .Distinct()
                .Count();

            var sessions = allMockSessions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new GetRetailerPaginatedSessionsResponse(sessions, totalCount, uniqueUsersCount, request.PageNumber, request.PageSize);
        }
    }
}
