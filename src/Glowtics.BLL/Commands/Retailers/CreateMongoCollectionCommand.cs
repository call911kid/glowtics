using Glowtics.BLL.Exceptions;
using MediatR;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Glowtics.BLL.Commands.Retailers
{
    public record CreateMongoCollectionCommand(string CollectionName) : IRequest;

    public class CreateMongoCollectionCommandHandler : IRequestHandler<CreateMongoCollectionCommand>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public CreateMongoCollectionCommandHandler(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public async Task Handle(CreateMongoCollectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _mongoDatabase.CreateCollectionAsync(request.CollectionName, cancellationToken: cancellationToken);
            }
            catch (MongoException ex)
            {
                throw new DatabaseProvisioningException("Failed to provision the retailer's catalog database. Please try again later.", ex);
            }

        }
    }
}
