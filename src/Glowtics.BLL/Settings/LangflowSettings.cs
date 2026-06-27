namespace Glowtics.BLL.Settings
{
    public class LangflowSettings
    {
        public const string SectionName = "LangflowApi";
        public string BaseUrl { get; init; } = null!;
        public string ApiKey { get; init; } = null!;
        public string FlowId { get; init; } = null!;
        public string? NgrokUser { get; init; }
        public string? NgrokPass { get; init; }
        public string ChatInputNodeId { get; init; } = "ChatInput";
    }
}
