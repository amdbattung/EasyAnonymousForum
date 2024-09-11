using EasyAnonymousForum.Server.Features.Comments.DTOs;
using FluentValidation;

namespace EasyAnonymousForum.Server.Features.Comments.Validators
{
    public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentValidator()
        {
            
        }
    }
}
