using System.Text.Json.Serialization;

namespace EasyAnonymousForum.Server.Features.Threads.DTOs
{
    [Serializable]
    public class GetManyThreadDto
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public int ItemCount { get; }
        [JsonPropertyName("data")]
        public IEnumerable<GetThreadDto> Threads { get; }

        public GetManyThreadDto(int pageNumber, int pageSize, int itemCount, IEnumerable<GetThreadDto> threads)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.ItemCount = itemCount;
            this.Threads = threads;
        }
    }
}
