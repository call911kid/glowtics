using System;
using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public class UpdateRetailerProfileRequestDto
    {
        public string? Domain { get; set; }
        public string? CartRedirectUrl { get; set; }
        public string? BrandLogoUrl { get; set; }
    }

    public class UpdateRetailerProfileRequestDtoValidator : AbstractValidator<UpdateRetailerProfileRequestDto>
    {
        public UpdateRetailerProfileRequestDtoValidator()
        {
            RuleFor(x => x.Domain)
                .Matches(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage("Must be a valid domain name.")
                .When(x => !string.IsNullOrEmpty(x.Domain));

            RuleFor(x => x.CartRedirectUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Must be a valid absolute URL.")
                .When(x => !string.IsNullOrEmpty(x.CartRedirectUrl));

            RuleFor(x => x.BrandLogoUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Must be a valid absolute URL.")
                .When(x => !string.IsNullOrEmpty(x.BrandLogoUrl));
        }
    }
}
