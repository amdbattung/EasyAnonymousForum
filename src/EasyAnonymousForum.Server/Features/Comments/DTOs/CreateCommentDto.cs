using EasyAnonymousForum.Server.Models;
using NodaTime;

namespace EasyAnonymousForum.Server.Features.Comments.DTOs
{
    [Serializable]
    public class CreateCommentDto
    {
        public string? ThreadId { get; set; }
        public string? Content { get; set; }
    }
}
