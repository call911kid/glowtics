namespace Glowtics.Api.DTOs.Requests
{
    public record ResetPasswordRequestDto(string Email, string Otp, string NewPassword);
}
