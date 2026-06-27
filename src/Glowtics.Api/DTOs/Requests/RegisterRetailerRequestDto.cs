using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public class RegisterRetailerRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }

    public class RegisterRetailerRequestDtoValidator : AbstractValidator<RegisterRetailerRequestDto>
    {
        public RegisterRetailerRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Must be a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Must be at least 8 characters.");

            RuleFor(x => x.Domain)
                .NotEmpty().WithMessage("Domain is required.")
                .Matches(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Must be a valid domain name.");
        }
    }
}
