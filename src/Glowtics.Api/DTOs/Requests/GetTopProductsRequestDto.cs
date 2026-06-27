using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public class GetTopProductsRequestDto
    {
        public int Limit { get; set; } = 5;
    }

    public class GetTopProductsRequestDtoValidator : AbstractValidator<GetTopProductsRequestDto>
    {
        public GetTopProductsRequestDtoValidator()
        {
            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Limit must be greater than 0.")
                .LessThanOrEqualTo(50).WithMessage("Limit must be less than or equal to 50.");
        }
    }
}
