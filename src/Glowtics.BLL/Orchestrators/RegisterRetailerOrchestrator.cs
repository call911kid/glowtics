using Glowtics.BLL.Commands.Identity;
using Glowtics.BLL.Commands.Retailers;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Responses;
using Glowtics.DAL.Context;
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
            bool mongoCollectionCreated = false;
            string collectionName = string.Empty;

            try
            {
                var userId = await _mediator.Send(new CreateGlowticsUserCommand(request.Email, request.Password, Roles.Retailer), cancellationToken);

                collectionName = $"catalog_{userId}";
                await _mediator.Send(new CreateMongoCollectionCommand(collectionName), cancellationToken);
                mongoCollectionCreated = true;

                var retailerProfile = await _mediator.Send(new CreateRetailerProfileCommand(userId, request.Domain, collectionName), cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                await _mediator.Send(new SendConfirmationEmailCommand(userId), cancellationToken);

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
                
                if (mongoCollectionCreated && !string.IsNullOrEmpty(collectionName))
                {
                    await _mediator.Send(new DeleteMongoCollectionCommand(collectionName), CancellationToken.None);
                }

                throw;
            }
        }
    }
}
