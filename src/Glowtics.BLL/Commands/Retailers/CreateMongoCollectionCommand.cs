using Glowtics.BLL.Exceptions;
using MediatR;
using MongoDB.Bson;
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

                // Provision the Atlas vector index the analyze flow's RAG step queries (Cohere = 384-d, cosine,
                // path "embedding"). Without it, $vectorSearch over this retailer's catalog returns nothing.
                // Raw createSearchIndexes command so it works regardless of driver version.
                var createIndex = new BsonDocument
                {
                    { "createSearchIndexes", request.CollectionName },
                    { "indexes", new BsonArray
                        {
                            new BsonDocument
                            {
                                { "name", "vector_index" },
                                { "type", "vectorSearch" },
                                { "definition", new BsonDocument
                                    {
                                        { "fields", new BsonArray
                                            {
                                                new BsonDocument
                                                {
                                                    { "type", "vector" },
                                                    { "path", "embedding" },
                                                    { "numDimensions", 384 },
                                                    { "similarity", "cosine" }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                await _mongoDatabase.RunCommandAsync<BsonDocument>(createIndex, cancellationToken: cancellationToken);
            }
            catch (MongoException ex)
            {
                throw new DatabaseProvisioningException("Failed to provision the retailer's catalog database. Please try again later.", ex);
            }

        }
    }
}
