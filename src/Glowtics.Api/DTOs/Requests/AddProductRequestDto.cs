using System.Collections.Generic;

namespace Glowtics.Api.DTOs.Requests
{
    public class AddProductRequestDto
    {
        public string ExternalProductId { get; init; } = null!;
        public string Name { get; init; } = null!;
        public List<string> TargetConditions { get; init; } = new();
        public List<string> ActiveIngredients { get; init; } = new();
        public List<string> Conflicts { get; init; } = new();
        public List<string> ImageUrls { get; init; } = new();
    }
}
