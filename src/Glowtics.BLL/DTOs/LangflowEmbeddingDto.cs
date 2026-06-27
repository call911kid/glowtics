using System.Collections.Generic;

namespace Glowtics.BLL.DTOs
{
    public class LangflowEmbeddingDto
    {
        public string CollectionName { get; set; } = null!;
        public string ExternalProductId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<string> TargetConditions { get; init; } = new();
        public List<string> ActiveIngredients { get; init; } = new();
        public List<string> Conflicts { get; init; } = new();
        public bool IsAvailable { get; init; }
    }
}
