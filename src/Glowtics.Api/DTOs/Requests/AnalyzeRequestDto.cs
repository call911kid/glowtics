using Microsoft.AspNetCore.Http;

namespace Glowtics.Api.DTOs.Requests
{
    public class AnalyzeRequestDto
    {
        public IFormFile Photo { get; set; } = null!;
        public string Domain { get; set; } = string.Empty;
    }
}
