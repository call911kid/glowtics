namespace Glowtics.BLL.Settings
{
    public class LangflowSettings
    {
        public const string SectionName = "LangflowApi";
        public string BaseUrl { get; init; } = null!;
        public string ApiKey { get; init; } = null!;
        public string FlowId { get; init; } = null!;
    }
}
