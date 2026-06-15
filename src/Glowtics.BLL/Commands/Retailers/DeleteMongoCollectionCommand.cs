using MediatR;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Glowtics.BLL.Commands.Retailers
{
    public record DeleteMongoCollectionCommand(string CollectionName) : IRequest;

    public class DeleteMongoCollectionCommandHandler : IRequestHandler<DeleteMongoCollectionCommand>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public DeleteMongoCollectionCommandHandler(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public async Task Handle(DeleteMongoCollectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _mongoDatabase.DropCollectionAsync(request.CollectionName, cancellationToken);
            }
            catch (Exception)
            {
                //log later if it fails
            }
        }
    }
}
