using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Queries.Sessions
{
    public record GetSessionsByExternalUserIdQuery(
        Guid RetailerId, 
        string ExternalUserId, 
        int PageNumber = 1, 
        int PageSize = 10) : IRequest<GetSessionsByExternalUserIdResponse>;

    public record SessionProductDto(
        Guid Id, 
        string ExternalProductId, 
        string Name, 
        bool IsAvailable,
        List<string> TargetConditions,
        List<string> ActiveIngredients,
        List<string> Conflicts,
        List<string> ImageUrls
    );

    public record DiagnosticSessionDto(
        Guid Id, 
        string SkinProfileResult, 
        string? SkinProfileJson, 
        string? RoutineJson, 
        string? Feedback, 
        List<SessionProductDto> RecommendedProducts, 
        DateTime CreatedAt
    );

    public record GetSessionsByExternalUserIdResponse(
        List<DiagnosticSessionDto> Sessions, 
        int TotalCount, 
        int PageNumber, 
        int PageSize
    );

    public class GetSessionsByExternalUserIdQueryHandler : IRequestHandler<GetSessionsByExternalUserIdQuery, GetSessionsByExternalUserIdResponse>
    {
        private readonly GlowticsDbContext _context;

        public GetSessionsByExternalUserIdQueryHandler(GlowticsDbContext context)
        {
            _context = context;
        }

        public async Task<GetSessionsByExternalUserIdResponse> Handle(GetSessionsByExternalUserIdQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DiagnosticSessions
                .Where(s => s.RetailerId == request.RetailerId && s.ExternalUserId == request.ExternalUserId);

            var totalCount = await query.CountAsync(cancellationToken);

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
                    s.Feedback,
                    s.CreatedAt,
                    Products = s.RecommendedProducts.Select(p => new 
                    {
                        p.Id,
                        p.ExternalProductId,
                        p.Name,
                        p.IsAvailable,
                        p.TargetConditions,
                        p.ActiveIngredients,
                        p.Conflicts,
                        p.ImageUrls
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            var sessions = page.Select(s => new DiagnosticSessionDto(
                s.Id,
                s.SkinProfileResult,
                s.SkinProfileJson,
                s.RoutineJson,
                s.Feedback,
                s.Products.Select(p => new SessionProductDto(
                    p.Id,
                    p.ExternalProductId,
                    p.Name,
                    p.IsAvailable,
                    p.TargetConditions,
                    p.ActiveIngredients,
                    p.Conflicts,
                    p.ImageUrls
                )).ToList(),
                s.CreatedAt
            )).ToList();

            return new GetSessionsByExternalUserIdResponse(sessions, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
