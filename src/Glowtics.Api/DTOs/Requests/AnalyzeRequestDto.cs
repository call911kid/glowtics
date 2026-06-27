using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace Glowtics.Api.DTOs.Requests
{
    public class AnalyzeRequestDto
    {
        public IFormFile Photo { get; set; } = null!;
        public string Domain { get; set; } = string.Empty;
    }

    public class AnalyzeRequestDtoValidator : AbstractValidator<AnalyzeRequestDto>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

        public AnalyzeRequestDtoValidator()
        {
            RuleFor(x => x.Domain)
                .NotEmpty().WithMessage("Domain is required.")
                .Matches(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage("Must be a valid domain name.");

            RuleFor(x => x.Photo)
                .NotNull().WithMessage("Photo is required.")
                .Must(photo => photo == null || photo.Length > 0).WithMessage("Photo cannot be empty.")
                .Must(photo => photo == null || photo.Length <= 10 * 1024 * 1024).WithMessage("Photo must be less than 10 MB.")
                .Must(photo => photo == null || AllowedContentTypes.Contains(photo.ContentType.ToLower())).WithMessage("Unsupported file type. Accepted formats: JPEG, PNG, WebP.")
                .Must(photo =>
                {
                    if (photo == null) return true;
                    var extension = Path.GetExtension(photo.FileName).ToLower();
                    return AllowedExtensions.Contains(extension);
                }).WithMessage("Unsupported file extension. Accepted formats: .jpg, .jpeg, .png, .webp.");
        }
    }
}
