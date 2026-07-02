using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    /// <summary>Step 2 of analyze: turn the step-1 skin profile into a product routine.</summary>
    public class BuildRoutineRequestDto
    {
        public string SkinProfile { get; set; } = string.Empty;
        public string? ImageHash { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string? ExternalUserId { get; set; }
    }

    public class BuildRoutineRequestDtoValidator : AbstractValidator<BuildRoutineRequestDto>
    {
        public BuildRoutineRequestDtoValidator()
        {
            RuleFor(x => x.SkinProfile).NotEmpty().WithMessage("Skin profile is required.");
            RuleFor(x => x.Domain)
                .NotEmpty().WithMessage("Domain is required.")
                .Matches(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Must be a valid domain name.");
        }
    }
}
