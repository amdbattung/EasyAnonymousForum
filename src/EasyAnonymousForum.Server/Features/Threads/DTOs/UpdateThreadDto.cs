using NodaTime;

namespace EasyAnonymousForum.Server.Features.Threads.DTOs
{
    [Serializable]
    public class UpdateThreadDto
    {
        public string? Title { get; set; }
        public string? TopicId { get; set; }
        public string? Body { get; set; }
    }
}
