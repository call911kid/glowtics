using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Interfaces;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Glowtics.BLL.Commands.Embeddings
{
    public record AddEmbeddingCommand(
        string CollectionName,
        string ExternalProductId,
        string Name,
        List<string> TargetConditions,
        List<string> ActiveIngredients,
        List<string> Conflicts
    ) : IRequest<bool>;

    /// <summary>
    /// Embeds the product with Cohere (backend-side) and writes the vector doc straight into the
    /// retailer's Mongo collection. Field names match DeleteEmbeddingCommand / UpdateEmbeddingAvailabilityCommand
    /// (ExternalProductId, IsAvailable) and the analyze flow's RAG side (text, embedding).
    /// </summary>
    public class AddEmbeddingCommandHandler : IRequestHandler<AddEmbeddingCommand, bool>
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IMongoDatabase _mongoDatabase;

        public AddEmbeddingCommandHandler(IEmbeddingService embeddingService, IMongoDatabase mongoDatabase)
        {
            _embeddingService = embeddingService;
            _mongoDatabase = mongoDatabase;
        }

        public async Task<bool> Handle(AddEmbeddingCommand request, CancellationToken cancellationToken)
        {
            var conflicts = request.Conflicts is { Count: > 0 } ? request.Conflicts : new List<string> { "None" };
            var text = BuildText(request, conflicts);

            var embedding = await _embeddingService.EmbedDocumentAsync(text, cancellationToken);

            var doc = new BsonDocument
            {
                { "ExternalProductId", request.ExternalProductId },
                { "Name", request.Name ?? string.Empty },
                { "TargetConditions", new BsonArray(request.TargetConditions ?? new List<string>()) },
                { "ActiveIngredients", new BsonArray(request.ActiveIngredients ?? new List<string>()) },
                { "Conflicts", new BsonArray(conflicts) },
                { "IsAvailable", true },
                { "text", text },
                { "embedding", new BsonArray(embedding.Select(f => (double)f)) }
            };

            var collection = _mongoDatabase.GetCollection<BsonDocument>(request.CollectionName);
            await collection.InsertOneAsync(doc, cancellationToken: cancellationToken);
            return true;
        }

        // Same shape as langflow/ingest_catalog.py to_text() so the embedding matches the RAG query side.
        // (Product has no Category in this schema, so it is omitted.)
        private static string BuildText(AddEmbeddingCommand p, List<string> conflicts)
        {
            string join(List<string>? xs) => xs is { Count: > 0 } ? string.Join(", ", xs.Where(s => !string.IsNullOrWhiteSpace(s))) : "None";
            return $"Product {p.ExternalProductId}: {p.Name}. " +
                   $"Targets: {join(p.TargetConditions)}. " +
                   $"Active ingredients: {join(p.ActiveIngredients)}. " +
                   $"Conflicts: {join(conflicts)}.";
        }
    }
}
