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

    public record ProductCountDto(
        Guid ProductId, 
        string ProductName, 
        int RecommendationCount,
        List<string> TargetConditions,
        List<string> ActiveIngredients,
        List<string> Conflicts,
        List<string> ImageUrls);

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

            // --- MOCK DATA ---
            var mockTopProducts = new List<ProductCountDto>
            {
                new ProductCountDto(
                    Guid.NewGuid(), "Vitamin C Serum", 124, 
                    new List<string> { "Dullness", "Aging" }, 
                    new List<string> { "Vitamin C", "Hyaluronic Acid" }, 
                    new List<string> { "Retinol", "AHA" }, 
                    new List<string> { "https://glowtics.com/images/serum.jpg" }),
                new ProductCountDto(
                    Guid.NewGuid(), "Hydrating Cleanser", 98,
                    new List<string> { "Dryness", "Sensitive Skin" },
                    new List<string> { "Ceramides", "Glycerin" },
                    new List<string>(),
                    new List<string> { "https://glowtics.com/images/cleanser.jpg" }),
                new ProductCountDto(
                    Guid.NewGuid(), "Oil-Free Moisturizer", 76,
                    new List<string> { "Acne", "Oiliness" },
                    new List<string> { "Niacinamide" },
                    new List<string>(),
                    new List<string> { "https://glowtics.com/images/moisturizer.jpg" }),
                new ProductCountDto(
                    Guid.NewGuid(), "Exfoliating Toner", 45,
                    new List<string> { "Acne", "Uneven Texture" },
                    new List<string> { "Salicylic Acid", "Glycolic Acid" },
                    new List<string> { "Vitamin C" },
                    new List<string> { "https://glowtics.com/images/toner.jpg" }),
                new ProductCountDto(
                    Guid.NewGuid(), "Mineral Sunscreen SPF 50", 32,
                    new List<string> { "Sun Protection", "Aging" },
                    new List<string> { "Zinc Oxide" },
                    new List<string>(),
                    new List<string> { "https://glowtics.com/images/sunscreen.jpg" })
            };

            var topProducts = mockTopProducts
                .OrderByDescending(x => x.RecommendationCount)
                .Take(request.Limit)
                .ToList();

            return new GetRetailerTopProductsResponse(topProducts);
        }
    }
}
