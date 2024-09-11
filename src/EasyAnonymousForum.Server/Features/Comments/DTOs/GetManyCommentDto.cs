using EasyAnonymousForum.Server.Features.Threads.DTOs;
using System.Text.Json.Serialization;

namespace EasyAnonymousForum.Server.Features.Comments.DTOs
{
    [Serializable]
    public class GetManyCommentDto
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public int ItemCount { get; }
        [JsonPropertyName("data")]
        public IEnumerable<GetCommentDto> Comments { get; }

        public GetManyCommentDto(int pageNumber, int pageSize, int itemCount, IEnumerable<GetCommentDto> comments)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.ItemCount = itemCount;
            this.Comments = comments;
        }
    }
}
