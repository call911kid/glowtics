using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Glowtics.BLL.Commands.Embeddings
{
    public record UpdateEmbeddingAvailabilityCommand(
        string CollectionName,
        string ExternalProductId,
        bool IsAvailable
    ) : IRequest<bool>;

    public class UpdateEmbeddingAvailabilityCommandHandler : IRequestHandler<UpdateEmbeddingAvailabilityCommand, bool>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public UpdateEmbeddingAvailabilityCommandHandler(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public async Task<bool> Handle(UpdateEmbeddingAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var collection = _mongoDatabase.GetCollection<BsonDocument>(request.CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("ExternalProductId", request.ExternalProductId);
            var update = Builders<BsonDocument>.Update.Set("IsAvailable", request.IsAvailable);

            var result = await collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
