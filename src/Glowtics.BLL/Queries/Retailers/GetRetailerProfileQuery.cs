using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Queries.Retailers
{
    public record GetRetailerProfileQuery(Guid UserId) : IRequest<GetRetailerProfileResponse>;

    public class GetRetailerProfileResponse
    {
        public Guid RetailerId { get; set; }
        public string Domain { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? BrandLogoUrl { get; set; }
        public string? CartRedirectUrl { get; set; }
        public string? ApiKeyHint { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetRetailerProfileQueryHandler : IRequestHandler<GetRetailerProfileQuery, GetRetailerProfileResponse>
    {
        private readonly GlowticsDbContext _dbContext;

        public GetRetailerProfileQueryHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetRetailerProfileResponse> Handle(GetRetailerProfileQuery request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.UserId == request.UserId && !r.IsDeleted, cancellationToken)
                ?? throw new RetailerNotFoundException($"Retailer profile not found for user ({request.UserId}).");

            return new GetRetailerProfileResponse
            {
                RetailerId = retailer.Id,
                Domain = retailer.Domain,
                Status = retailer.Status.ToString(),
                BrandLogoUrl = retailer.BrandLogoUrl,
                CartRedirectUrl = retailer.CartRedirectUrl,
                ApiKeyHint = retailer.ApiKeyHint,
                CreatedAt = retailer.CreatedAt
            };
        }
    }
}
