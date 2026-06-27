using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Glowtics.BLL.DTOs
{
    public class LangflowRoutineResponse
    {
        [JsonPropertyName("bundle_name")]
        public string BundleName { get; set; } = string.Empty;

        [JsonPropertyName("routine")]
        public List<LangflowRoutineItem> Routine { get; set; } = new List<LangflowRoutineItem>();
    }

    public class LangflowRoutineItem
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("rationale")]
        public string Rationale { get; set; } = string.Empty;
    }
}
