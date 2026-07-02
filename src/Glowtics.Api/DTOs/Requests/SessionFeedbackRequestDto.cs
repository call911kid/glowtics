using System;
using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public class SessionFeedbackRequestDto
    {
        public Guid SessionId { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class SessionFeedbackRequestDtoValidator : AbstractValidator<SessionFeedbackRequestDto>
    {
        public SessionFeedbackRequestDtoValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId is required.");

            RuleFor(x => x.Feedback)
                .NotEmpty().WithMessage("Feedback is required.")
                .MaximumLength(2000).WithMessage("Feedback must be 2000 characters or fewer.");
        }
    }
}
