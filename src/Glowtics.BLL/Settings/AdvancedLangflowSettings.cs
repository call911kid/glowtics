namespace Glowtics.BLL.Settings
{
    public class AdvancedLangflowSettings
    {
        public const string SectionName = "LangflowApiAdvanced";

        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ValidationFlowId { get; set; } = string.Empty;
        public string AnalysisFlowId { get; set; } = string.Empty;
        // 3rd flow: skin profile text -> RAG + rerank -> product routine (GLOWTICS Build Routine.json).
        public string BuildRoutineFlowId { get; set; } = string.Empty;
        public string? AgentFlowId { get; set; }
    }
}
