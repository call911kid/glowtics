using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);

    public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordRequestDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Must be at least 8 characters.");
        }
    }
}
