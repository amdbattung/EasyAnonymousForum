using System.Text.Json.Serialization;

namespace EasyAnonymousForum.Server.Features.Topics.DTOs
{
    [Serializable]
    public class GetManyTopicDto
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public int ItemCount { get; }
        [JsonPropertyName("data")]
        public IEnumerable<GetTopicDto> Topics { get; }

        public GetManyTopicDto(int pageNumber, int pageSize, int itemCount, IEnumerable<GetTopicDto> topics)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.ItemCount = itemCount;
            this.Topics = topics;
        }
    }
}
