using NodaTime;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EasyAnonymousForum.Server.Features.Threads.DTOs;

namespace EasyAnonymousForum.Server.Models
{
    public class ForumThread
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public required string Id { get; set; }
        public required string Title { get; set; }
        public Topic? Topic { get; set; }
        public required string Body { get; set; }
        public Instant DatePosted { get; set; }

        public GetThreadDto ToDto()
        {
            return new GetThreadDto(
                id: this.Id,
                title: this.Title,
                topicId: this.Topic?.Id,
                body: this.Body,
                datePosted: this.DatePosted
            );
        }
    }
}
