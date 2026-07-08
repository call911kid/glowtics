using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Glowtics.DAL.Context;
using Glowtics.BLL.Responses;
using Glowtics.BLL.Interfaces;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Constants;

namespace Glowtics.BLL.Commands.Retailers
{
    public record RotateCatalogApiKeyCommand(Guid UserId) : IRequest<RotateCatalogApiKeyResponse>;

    public class RotateCatalogApiKeyCommandHandler : IRequestHandler<RotateCatalogApiKeyCommand, RotateCatalogApiKeyResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly IApiKeyService _apiKeyService;

        public RotateCatalogApiKeyCommandHandler(GlowticsDbContext dbContext, IApiKeyService apiKeyService)
        {
            _dbContext = dbContext;
            _apiKeyService = apiKeyService;
        }

        public async Task<RotateCatalogApiKeyResponse> Handle(RotateCatalogApiKeyCommand request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers.FirstOrDefaultAsync(r => r.UserId == request.UserId, cancellationToken)
                ?? throw new RetailerNotFoundException($"Retailer profile for user {request.UserId} not found.");
           
            var generatedKey = _apiKeyService.GenerateApiKey();

            retailer.ApiKeyHash = generatedKey.Hash;
            retailer.ApiKeyHint = generatedKey.Hint;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RotateCatalogApiKeyResponse
            {
                RawKey = generatedKey.RawKey,
                Hint = generatedKey.Hint
            };
        }
    }
}

