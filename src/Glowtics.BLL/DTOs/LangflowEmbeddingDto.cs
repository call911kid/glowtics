using System.Collections.Generic;

namespace Glowtics.BLL.DTOs
{
    public class LangflowEmbeddingDto
    {
        public string CollectionName { get; init; } = null!;
        public string ExternalProductId { get; init; } = null!;
        public List<string> TargetConditions { get; init; } = new();
        public List<string> ActiveIngredients { get; init; } = new();
        public List<string> Conflicts { get; init; } = new();
        public bool IsAvailable { get; init; }
    }
}
