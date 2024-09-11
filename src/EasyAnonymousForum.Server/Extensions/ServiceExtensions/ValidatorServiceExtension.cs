using EasyAnonymousForum.Server.Features.Comments.DTOs;
using EasyAnonymousForum.Server.Features.Comments.Validators;
using EasyAnonymousForum.Server.Features.Queries;
using EasyAnonymousForum.Server.Features.Threads.DTOs;
using EasyAnonymousForum.Server.Features.Threads.Validators;
using EasyAnonymousForum.Server.Features.Topics.DTOs;
using EasyAnonymousForum.Server.Features.Topics.Validators;
using FluentValidation;

namespace EasyAnonymousForum.Server.Extensions.ServiceExtensions
{
    public static class ValidatorServiceExtension
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Register validators here
            services.AddScoped<IValidator<CreateTopicDto>, CreateTopicValidator>();
            services.AddScoped<IValidator<UpdateTopicDto>, UpdateTopicValidator>();
            services.AddScoped<IValidator<CreateThreadDto>, CreateThreadValidator>();
            services.AddScoped<IValidator<UpdateThreadDto>, UpdateThreadValidator>();
            services.AddScoped<IValidator<CreateCommentDto>, CreateCommentValidator>();
            services.AddScoped<IValidator<UpdateCommentDto>, UpdateCommentValidator>();
            services.AddScoped<IValidator<QueryObject>, QueryObjectValidator>();

            return services;
        }
    }
}
