using EasyAnonymousForum.Server.Features.Threads.DTOs;
using FluentValidation;

namespace EasyAnonymousForum.Server.Features.Threads.Validators
{
    public class CreateThreadValidator : AbstractValidator<CreateThreadDto>
    {
        public CreateThreadValidator()
        {
            
        }
    }
}
