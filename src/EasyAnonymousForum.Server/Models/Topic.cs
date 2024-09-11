using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EasyAnonymousForum.Server.Features.Topics.DTOs;

namespace EasyAnonymousForum.Server.Models
{
    public class Topic
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public required string Id { get; set; }
        public required string Name { get; set; }

        public GetTopicDto ToDto()
        {
            return new GetTopicDto(
                id: this.Id,
                name: this.Name
            );
        }
    }
}
