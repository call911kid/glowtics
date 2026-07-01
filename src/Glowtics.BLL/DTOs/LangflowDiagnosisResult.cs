using System.Collections.Generic;

namespace Glowtics.BLL.DTOs
{
    public class LangflowDiagnosisResult
    {
        /// <summary>True when the photo passed validation and a routine was produced.</summary>
        public bool IsValidFace { get; set; }

        /// <summary>User-facing reason when IsValidFace is false (e.g. "Image rejected...").</summary>
        public string RejectionReason { get; set; } = string.Empty;

        /// <summary>The full routine/skin-profile JSON returned by the flow (when valid).</summary>
        public string SkinProfileResult { get; set; } = string.Empty;

        /// <summary>Catalog product ids the flow recommended (when valid).</summary>
        public List<string> ExternalProductIds { get; set; } = new List<string>();

        public List<LangflowRoutineItem> RoutineItems { get; set; } = new List<LangflowRoutineItem>();
    }
}
