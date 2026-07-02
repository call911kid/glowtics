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
        string? ExternalUserId,
        string? ImageHash = null,
        string? SkinProfileJson = null,
        string? RoutineJson = null
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
            // 1. Fetch the Retailer to ensure it exists
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Id == request.RetailerId, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"Entity 'Retailer' ({request.RetailerId}) was not found.");

            // 2. Fetch the corresponding Products tracked by DbContext using the ExternalProductIds
            var products = await _dbContext.Products
                .Where(p => p.RetailerId == request.RetailerId && request.ExternalProductIds.Contains(p.ExternalProductId))
                .ToListAsync(cancellationToken);

            // 3. Create the new DiagnosticSession entity
            var session = new DiagnosticSession
            {
                RetailerId = retailer.Id,
                SkinProfileResult = request.SkinProfileResult,
                SkinProfileJson = request.SkinProfileJson,
                RoutineJson = request.RoutineJson,
                ExternalUserId = request.ExternalUserId,
                ImageHash = request.ImageHash,
                RecommendedProducts = new List<Product>(),
                CreatedAt = DateTime.UtcNow
            };

            // 4. Link the fetched Products to the session
            foreach (var product in products)
            {
                session.RecommendedProducts.Add(product);
            }

            // 5. Add to the DbContext and save
            _dbContext.DiagnosticSessions.Add(session);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AddDiagnosticSessionResponse { SessionId = session.Id };
        }
    }
}
