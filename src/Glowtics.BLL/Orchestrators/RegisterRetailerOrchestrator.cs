using Glowtics.BLL.Commands;
using Glowtics.BLL.Responses;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using Glowtics.DAL.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Glowtics.BLL.Orchestrators
{
    public record RegisterRetailerOrchestratorRequest(string Email, string Password, string Domain) : IRequest<RegisterRetailerResponse>;

    public class RegisterRetailerOrchestrator : IRequestHandler<RegisterRetailerOrchestratorRequest, RegisterRetailerResponse>
    {
        private readonly IMediator _mediator;
        private readonly GlowticsDbContext _dbContext;

        public RegisterRetailerOrchestrator(IMediator mediator, GlowticsDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        public async Task<RegisterRetailerResponse> Handle(RegisterRetailerOrchestratorRequest request, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var userId = await _mediator.Send(new Commands.Identity.CreateGlowticsUserCommand(request.Email, request.Password, "Retailer"), cancellationToken);

                
                // var collectionName = await _mediator.Send(new CreateMongoCollectionCommand(request.Domain), cancellationToken);
                var collectionName = $"retailer_{userId}";

                var retailerProfile = await _mediator.Send(new Commands.Retailers.CreateRetailerProfileCommand(userId, request.Domain, collectionName), cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new RegisterRetailerResponse
                {
                    Id = retailerProfile.Id,
                    Email = request.Email, 
                    Domain = retailerProfile.Domain

                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
