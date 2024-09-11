using FluentValidation;

namespace EasyAnonymousForum.Server.Features.Queries
{
    public class QueryObjectValidator : AbstractValidator<QueryObject>
    {
        public QueryObjectValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .OverridePropertyName("pageNumber")
                .WithMessage("Invalid page number.")
                .WithErrorCode("QERR0001");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Invalid page size.")
                .LessThanOrEqualTo(50)
                .OverridePropertyName("pageSize")
                .WithMessage("Page size too large.")
                .WithErrorCode("QERR0002");
        }
    }
}
