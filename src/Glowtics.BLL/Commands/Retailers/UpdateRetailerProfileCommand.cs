using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using Glowtics.DAL.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Commands.Retailers
{
    public record UpdateRetailerProfileCommand(Guid UserId, string? Domain, string? CartRedirectUrl, string? BrandLogoUrl) : IRequest<bool>;

    public class UpdateRetailerProfileCommandHandler : IRequestHandler<UpdateRetailerProfileCommand, bool>
    {
        private readonly GlowticsDbContext _dbContext;

        public UpdateRetailerProfileCommandHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateRetailerProfileCommand request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.UserId == request.UserId && !r.IsDeleted, cancellationToken)
                ?? throw new RetailerNotFoundException("Retailer profile not found for this user.");

            if (!string.IsNullOrWhiteSpace(request.Domain))
            {
                retailer.Domain = request.Domain;
            }

            if (request.CartRedirectUrl != null)
            {
                retailer.CartRedirectUrl = request.CartRedirectUrl;
                
                if (retailer.Status == RetailerStatus.Pending && !string.IsNullOrWhiteSpace(retailer.CartRedirectUrl))
                {
                    retailer.Status = RetailerStatus.Active;
                }
            }

            if (request.BrandLogoUrl != null)
            {
                retailer.BrandLogoUrl = request.BrandLogoUrl;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
