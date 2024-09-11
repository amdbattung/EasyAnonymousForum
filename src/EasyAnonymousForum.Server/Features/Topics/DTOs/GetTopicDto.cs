namespace EasyAnonymousForum.Server.Features.Topics.DTOs
{
    [Serializable]
    public class GetTopicDto
    {
        public string? Id { get; }
        public string? Name { get; }

        public GetTopicDto(string? id, string? name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
