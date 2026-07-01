using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public class AddFeedbackRequestDto
    {
        public string ExternalUserId { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
    }

    public class AddFeedbackRequestDtoValidator : AbstractValidator<AddFeedbackRequestDto>
    {
        public AddFeedbackRequestDtoValidator()
        {
            RuleFor(x => x.ExternalUserId)
                .NotEmpty().WithMessage("ExternalUserId is required.")
                .MaximumLength(255).WithMessage("ExternalUserId cannot exceed 255 characters.");

            RuleFor(x => x.Feedback)
                .NotEmpty().WithMessage("Feedback is required.")
                .MaximumLength(2048).WithMessage("Feedback cannot exceed 2048 characters.");
        }
    }
}
