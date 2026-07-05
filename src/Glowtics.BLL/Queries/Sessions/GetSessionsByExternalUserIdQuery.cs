using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Queries.Sessions
{
    /// <summary>Shopper-facing session history, keyed by the (unguessable) externalUserId the host app owns.
    /// Anonymous by design — same trust model as the session-feedback endpoint. Optionally scoped to a
    /// single store via <paramref name="Domain"/>; when null, returns the user's sessions across retailers.</summary>
    public record GetSessionsByExternalUserIdQuery(
        string ExternalUserId,
        string? Domain = null,
        int PageNumber = 1,
        int PageSize = 10) : IRequest<SessionHistoryPagedResult>;

    // The three records below mirror the host app's SessionHistoryDto / PagedResult contract by shape:
    // Items[] { SessionId, CreatedAt, ImageUrl, SkinAnalysis { SkinType, Concerns }, Routine[] { ProductName, Step, ProductId } }.
    public record SessionHistoryPagedResult(
        List<SessionHistoryItem> Items,
        int PageNumber,
        int PageSize,
        int TotalCount);

    public record SessionHistoryItem(
        string SessionId,
        DateTime CreatedAt,
        string? ImageUrl,
        SkinAnalysisDto SkinAnalysis,
        List<RoutineItemDto> Routine);

    public record SkinAnalysisDto(string SkinType, List<string> Concerns);

    public record RoutineItemDto(string ProductName, string Step, string? ProductId);

    public class GetSessionsByExternalUserIdQueryHandler : IRequestHandler<GetSessionsByExternalUserIdQuery, SessionHistoryPagedResult>
    {
        private readonly GlowticsDbContext _context;

        public GetSessionsByExternalUserIdQueryHandler(GlowticsDbContext context)
        {
            _context = context;
        }

        public async Task<SessionHistoryPagedResult> Handle(GetSessionsByExternalUserIdQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DiagnosticSessions
                .Include(s => s.RecommendedProducts)
                .Where(s => s.ExternalUserId == request.ExternalUserId);

            if (!string.IsNullOrWhiteSpace(request.Domain))
            {
                var domain = request.Domain.Trim();
                query = query.Where(s => s.Retailer.Domain == domain);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var page = Math.Max(1, request.PageNumber);
            var size = Math.Clamp(request.PageSize, 1, 100);

            var sessions = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(cancellationToken);

            var items = sessions.Select(MapSession).ToList();
            return new SessionHistoryPagedResult(items, page, size, totalCount);
        }

        private static SessionHistoryItem MapSession(DiagnosticSession s) => new(
            s.Id.ToString(),
            s.CreatedAt,
            ImageUrl: null, // we don't persist the uploaded photo (deleted immediately), so no URL to expose
            ParseSkinAnalysis(s.SkinProfileJson, s.SkinProfileResult),
            // Routine = the persisted matched products (real name + our ExternalProductId). Step isn't tracked.
            s.RecommendedProducts
                .Select(p => new RoutineItemDto(p.Name, string.Empty, p.ExternalProductId))
                .ToList());

        /// <summary>Pull overallSkinType + a flat concern list out of the step-1 profile JSON
        /// (primaryConcerns if present, else the union of per-zone concerns). Falls back to the short badge.</summary>
        private static SkinAnalysisDto ParseSkinAnalysis(string? skinProfileJson, string? fallbackType)
        {
            var skinType = string.IsNullOrWhiteSpace(fallbackType) ? "Unknown" : fallbackType!;
            var concerns = new List<string>();

            if (!string.IsNullOrWhiteSpace(skinProfileJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(skinProfileJson);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("overallSkinType", out var t) && t.GetString() is { Length: > 0 } st)
                    {
                        skinType = st;
                    }

                    if (root.TryGetProperty("primaryConcerns", out var pc) && pc.ValueKind == JsonValueKind.Array)
                    {
                        AddConcerns(pc, concerns);
                    }
                    else if (root.TryGetProperty("zones", out var zones) && zones.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var zone in zones.EnumerateObject())
                        {
                            if (zone.Value.ValueKind == JsonValueKind.Object
                                && zone.Value.TryGetProperty("concerns", out var zc) && zc.ValueKind == JsonValueKind.Array)
                            {
                                AddConcerns(zc, concerns);
                            }
                        }
                    }
                }
                catch (JsonException) { /* non-JSON / legacy profile — keep the fallback type, no concerns */ }
            }

            return new SkinAnalysisDto(skinType, concerns);
        }

        private static void AddConcerns(JsonElement array, List<string> into)
        {
            foreach (var c in array.EnumerateArray())
            {
                if (c.ValueKind == JsonValueKind.String && c.GetString() is { Length: > 0 } cs
                    && !into.Contains(cs, StringComparer.OrdinalIgnoreCase))
                {
                    into.Add(cs);
                }
            }
        }
    }
}
