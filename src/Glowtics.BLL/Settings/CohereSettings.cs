namespace Glowtics.BLL.Settings
{
    /// <summary>Cohere embedding config. Model + dims MUST match the analyze flow's RAG query side
    /// (embed-english-light-v3.0, 384-d) or vector search returns garbage.</summary>
    public class CohereSettings
    {
        public const string SectionName = "Cohere";

        public string ApiKey { get; init; } = null!;
        public string Model { get; init; } = "embed-english-light-v3.0";
        public int Dimensions { get; init; } = 384;
    }
}
