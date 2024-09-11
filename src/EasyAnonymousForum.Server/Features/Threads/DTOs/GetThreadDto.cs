using EasyAnonymousForum.Server.Models;
using NodaTime;

namespace EasyAnonymousForum.Server.Features.Threads.DTOs
{
    [Serializable]
    public class GetThreadDto
    {
        public string? Id { get; }
        public string? Title { get; }
        public string? TopicId { get; }
        public string? Body { get; }
        public Instant? DatePosted { get; }

        public GetThreadDto(string? id, string? title, string? topicId, string? body, Instant? datePosted)
        {
            this.Id = id;
            this.Title = title;
            this.TopicId = topicId;
            this.Body = body;
            this.DatePosted = datePosted;
        }
    }
}
