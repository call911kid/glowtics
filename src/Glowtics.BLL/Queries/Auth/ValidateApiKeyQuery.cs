using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.DAL.Context;
using Glowtics.DAL.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Glowtics.BLL.Queries.Auth
{
    public record ValidateApiKeyResponse(Guid UserId, Guid RetailerId);

    public record ValidateApiKeyQuery(string RawKey) : IRequest<ValidateApiKeyResponse>;

    public class ValidateApiKeyQueryHandler : IRequestHandler<ValidateApiKeyQuery, ValidateApiKeyResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly IApiKeyService _apiKeyService;

        public ValidateApiKeyQueryHandler(GlowticsDbContext dbContext, IApiKeyService apiKeyService)
        {
            _dbContext = dbContext;
            _apiKeyService = apiKeyService;
        }

        public async Task<ValidateApiKeyResponse> Handle(ValidateApiKeyQuery request, CancellationToken cancellationToken)
        {
            var hashedKey = _apiKeyService.HashApiKey(request.RawKey);

            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.ApiKeyHash == hashedKey, cancellationToken)
                ?? throw new InvalidCredentialsException("Invalid API Key.");

            if (retailer.Status == RetailerStatus.Suspended || retailer.Status == RetailerStatus.Deactivated)
            {
                throw new AccountRestrictedException("Retailer is inactive.");
            }

            return new ValidateApiKeyResponse(retailer.UserId, retailer.Id);
        }
    }
}
