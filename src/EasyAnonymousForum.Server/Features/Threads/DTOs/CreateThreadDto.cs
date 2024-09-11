using EasyAnonymousForum.Server.Models;
using NodaTime;

namespace EasyAnonymousForum.Server.Features.Threads.DTOs
{
    [Serializable]
    public class CreateThreadDto
    {
        public string? Title { get; set; }
        public string? TopicId { get; set; }
        public string? Body { get; set; }
    }
}
