using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Commands.Diagnostics
{
    public record AddDiagnosticSessionCommand(
        Guid RetailerId,
        string SkinProfileResult,
        List<string> ExternalProductIds,
        string? ExternalUserId
    ) : IRequest<AddDiagnosticSessionResponse>;

    public class AddDiagnosticSessionResponse
    {
        public Guid SessionId { get; set; }
    }

    public class AddDiagnosticSessionCommandHandler : IRequestHandler<AddDiagnosticSessionCommand, AddDiagnosticSessionResponse>
    {
        private readonly GlowticsDbContext _dbContext;

        public AddDiagnosticSessionCommandHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AddDiagnosticSessionResponse> Handle(AddDiagnosticSessionCommand request, CancellationToken cancellationToken)
        {
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Id == request.RetailerId, cancellationToken)
                ?? throw new RetailerNotFoundException($"Retailer ({request.RetailerId}) was not found.");

            var products = await _dbContext.Products
                .Where(p => p.RetailerId == request.RetailerId && request.ExternalProductIds.Contains(p.ExternalProductId))
                .ToListAsync(cancellationToken);

            var session = new DiagnosticSession
            {
                RetailerId = retailer.Id,
                SkinProfileResult = request.SkinProfileResult,
                ExternalUserId = request.ExternalUserId,
                RecommendedProducts = new List<Product>(),
                CreatedAt = DateTime.UtcNow
            };

            foreach (var product in products)
            {
                session.RecommendedProducts.Add(product);
            }

            _dbContext.DiagnosticSessions.Add(session);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AddDiagnosticSessionResponse { SessionId = session.Id };
        }
    }
}
