using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Glowtics.BLL.Documents
{
    /// <summary>Observability trace persisted to MongoDB (collection "Traces").</summary>
    public class TraceDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = "analyze";
        public long LatencyMs { get; set; }
        public bool Accepted { get; set; }
        public int ProductCount { get; set; }
        public string Collection { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}
