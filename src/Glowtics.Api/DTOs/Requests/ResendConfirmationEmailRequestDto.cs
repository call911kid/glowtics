using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public record ResendConfirmationEmailRequestDto(string Email);

    public class ResendConfirmationEmailRequestDtoValidator : AbstractValidator<ResendConfirmationEmailRequestDto>
    {
        public ResendConfirmationEmailRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Must be a valid email address.");
        }
    }
}
