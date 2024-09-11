using NodaTime;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EasyAnonymousForum.Server.Features.Threads.DTOs;
using EasyAnonymousForum.Server.Features.Comments.DTOs;

namespace EasyAnonymousForum.Server.Models
{
    public class Comment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public required string Id { get; set; }
        public required ForumThread Thread { get; set; }
        public required string Content { get; set; }
        public Instant DatePosted { get; set; }

        public GetCommentDto ToDto()
        {
            return new GetCommentDto(
                id: this.Id,
                threadId: this.Thread?.Id,
                content: this.Content,
                datePosted: this.DatePosted
            );
        }
    }
}
