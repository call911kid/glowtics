namespace Glowtics.Api.DTOs.Requests
{
    public class UpdateProductAvailabilityRequestDto
    {
        public string ExternalProductId { get; init; }
        public bool IsAvailable { get; init; }
    }
}
