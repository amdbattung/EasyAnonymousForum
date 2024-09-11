using EasyAnonymousForum.Server.Models;

namespace EasyAnonymousForum.Server.Features.Comments.DTOs
{
    [Serializable]
    public class UpdateCommentDto
    {
        public string? ThreadId { get; set; }
        public string? Content { get; set; }
    }
}
