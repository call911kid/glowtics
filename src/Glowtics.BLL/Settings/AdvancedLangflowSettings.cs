namespace Glowtics.BLL.Settings
{
    public class AdvancedLangflowSettings
    {
        public const string SectionName = "LangflowApiAdvanced";

        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ValidationFlowId { get; set; } = string.Empty;
        public string AnalysisFlowId { get; set; } = string.Empty;
        public string? AgentFlowId { get; set; }
    }
}
