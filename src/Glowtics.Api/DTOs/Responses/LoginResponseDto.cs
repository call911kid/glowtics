using System;

namespace Glowtics.Api.DTOs.Responses
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}
