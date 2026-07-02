using System.Collections.Generic;

namespace Glowtics.BLL.Orchestrators
{
    /// <summary>A catalog product recommended by the analyze flow, shaped for the client.</summary>
    public class RecommendedProductDto
    {
        public string Rationale { get; set; } = string.Empty;
        public string ExternalProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public List<string> ActiveIngredients { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
    }
}
