using FluentValidation;

namespace Glowtics.Api.DTOs.Requests
{
    public class PaginationRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PaginationRequestDtoValidator : AbstractValidator<PaginationRequestDto>
    {
        public PaginationRequestDtoValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize must be less than or equal to 100.");
        }
    }
}
