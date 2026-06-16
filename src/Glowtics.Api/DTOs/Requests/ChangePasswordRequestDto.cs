namespace Glowtics.Api.DTOs.Requests
{
    public record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);
}
