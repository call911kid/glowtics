namespace Glowtics.BLL.Settings
{
    public class LangflowEmbeddingSettings
    {
        public const string SectionName = "LangflowEmbeddingApi";
        public string BaseUrl { get; init; } = null!;
        public string WorkflowId { get; init; } = null!;
        public string? ApiKey { get; init; }
        public string? HfApiKey { get; init; }
        public string? MongoUri { get; init; }
        public string? DbName { get; init; }
        public string? CollectionName { get; init; }
        public string? NgrokUser { get; init; }
        public string? NgrokPass { get; init; }
    }
}
