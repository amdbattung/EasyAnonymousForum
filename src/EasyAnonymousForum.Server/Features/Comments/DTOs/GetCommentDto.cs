using EasyAnonymousForum.Server.Models;
using NodaTime;

namespace EasyAnonymousForum.Server.Features.Comments.DTOs
{
    [Serializable]
    public class GetCommentDto
    {
        public string? Id { get; }
        public string? ThreadId { get; }
        public string? Content { get; }
        public Instant? DatePosted { get; }

        public GetCommentDto(string? id, string? threadId, string? content, Instant? datePosted)
        {
            this.Id = id;
            this.ThreadId = threadId;
            this.Content = content;
            this.DatePosted = datePosted;
        }
    }
}
