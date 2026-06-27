using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public record ForgotPasswordRequestDto(string Email);

    public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Must be a valid email address.");
        }
    }
}
