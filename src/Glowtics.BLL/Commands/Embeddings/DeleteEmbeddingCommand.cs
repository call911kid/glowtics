using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Glowtics.BLL.Commands.Embeddings
{
    public record DeleteEmbeddingCommand(
        string CollectionName,
        string ExternalProductId
    ) : IRequest<bool>;

    public class DeleteEmbeddingCommandHandler : IRequestHandler<DeleteEmbeddingCommand, bool>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public DeleteEmbeddingCommandHandler(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public async Task<bool> Handle(DeleteEmbeddingCommand request, CancellationToken cancellationToken)
        {
            var collection = _mongoDatabase.GetCollection<BsonDocument>(request.CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("ExternalProductId", request.ExternalProductId);
            
            var result = await collection.DeleteOneAsync(filter, cancellationToken);
            
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}
