using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public record ResetPasswordRequestDto(string Email, string Otp, string NewPassword);

    public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Must be a valid email address.");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required.")
                .Matches(@"^\d{6}$").WithMessage("Must be exactly 6 digits.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Must be at least 8 characters.");
        }
    }
}
