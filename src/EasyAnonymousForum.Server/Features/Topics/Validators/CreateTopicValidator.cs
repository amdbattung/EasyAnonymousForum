using EasyAnonymousForum.Server.Features.Topics.DTOs;
using FluentValidation;

namespace EasyAnonymousForum.Server.Features.Topics.Validators
{
    public class CreateTopicValidator : AbstractValidator<CreateTopicDto>
    {
        public CreateTopicValidator()
        {
            RuleFor(x => x.Name)
                .Matches(@"^(?!\s*$)[ -~]{2,}$")
                .OverridePropertyName("name")
                .WithMessage("Invalid name.");
        }
    }
}
