using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public record ConfirmEmailRequestDto(string Email, string Otp);

    public class ConfirmEmailRequestDtoValidator : AbstractValidator<ConfirmEmailRequestDto>
    {
        public ConfirmEmailRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Must be a valid email address.");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required.")
                .Matches(@"^\d{6}$").WithMessage("Must be exactly 6 digits.");
        }
    }
}
