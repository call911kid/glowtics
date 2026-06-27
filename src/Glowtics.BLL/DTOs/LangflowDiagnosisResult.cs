using System.Collections.Generic;

namespace Glowtics.BLL.DTOs
{
    public class LangflowDiagnosisResult
    {
        public string SkinProfileResult { get; set; } = string.Empty;
        public List<string> ExternalProductIds { get; set; } = new List<string>();
        public List<LangflowRoutineItem> RoutineItems { get; set; } = new List<LangflowRoutineItem>();
    }
}
