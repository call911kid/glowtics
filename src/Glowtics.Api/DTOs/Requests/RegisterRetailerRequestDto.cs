namespace Glowtics.Api.DTOs.Requests
{
    public class RegisterRetailerRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }
}
