using EasyAnonymousForum.Data;
using EasyAnonymousForum.Server.Features.Queries;
using EasyAnonymousForum.Server.Features.Topics.DTOs;
using EasyAnonymousForum.Server.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace EasyAnonymousForum.Server.Features.Topics
{
    [ApiController]
    [Route("api/topics")]
    public class TopicController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IValidator<CreateTopicDto> _createTopicValidator;
        private readonly IValidator<UpdateTopicDto> _updateTopicValidator;
        private readonly IValidator<QueryObject> _queryObjectValidator;
        private readonly ILogger<TopicController> _logger;

        public TopicController(DataContext context,
            IValidator<CreateTopicDto> createTopicValidator,
            IValidator<UpdateTopicDto> updateTopicValidator,
            IValidator<QueryObject> queryObjectValidator,
            ILogger<TopicController> logger)
        {
            this._context = context;
            this._createTopicValidator = createTopicValidator;
            this._updateTopicValidator = updateTopicValidator;
            this._queryObjectValidator = queryObjectValidator;
            this._logger = logger;
        }

        [HttpGet(Name = "IndexTopics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetManyTopicDto>> IndexAsync([FromQuery] QueryObject? queryObject)
        {
            if (queryObject != null)
            {
                ValidationResult validationResult = await _queryObjectValidator.ValidateAsync(queryObject);

                foreach (var error in validationResult.Errors)
                {
                    if (error.ErrorCode == "QERR0001")
                    {
                        queryObject.PageNumber = null;
                    }

                    if (error.ErrorCode == "QERR0002")
                    {
                        queryObject.PageSize = null;
                    }
                }
            }

            var topics = _context.Topics.AsQueryable();

            topics = topics.OrderBy(t => EF.Property<Instant>(t, "DateCreated"));

            if (!string.IsNullOrWhiteSpace(queryObject?.Query))
            {
                topics = topics.Where(t => EF.Functions.ToTsVector("simple", t.Name).Matches(EF.Functions.ToTsQuery("simple", $"'{queryObject.Query}':*")));
            }

            int pageNumber = queryObject?.PageNumber ?? 1;
            int pageSize = queryObject?.PageSize ?? 10;

            return Ok(new GetManyTopicDto(
                topics: (await topics.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync())
                .Select(t => t.ToDto()),
                pageNumber : pageNumber,
                pageSize: pageSize,
                itemCount: await topics.CountAsync()
            ));
        }

        [HttpPost(Name = "CreateTopic")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetTopicDto>> CreateAsync([FromBody] CreateTopicDto topic)
        {
            ValidationResult validationResult = await _createTopicValidator.ValidateAsync(topic);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(this.ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Topic newTopic = new()
            {
                #nullable disable
                Id = null,
                #nullable restore
                Name = topic.Name?.Trim() ?? ""
            };

            await _context.Topics.AddAsync(newTopic);
            Topic? response = await _context.SaveChangesAsync() > 0 ? newTopic : null;
            return response == null ? BadRequest(ModelState) : CreatedAtAction(nameof(ShowAsync), new { id = response.Id }, response.ToDto());
        }

        [HttpGet("{id}", Name = "ShowTopic")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetTopicDto>> ShowAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            Topic? topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            return topic == null ? NotFound() : Ok(topic.ToDto());
        }

        [HttpPut("{id}", Name = "UpdateTopic")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetTopicDto>> UpdateAsync([FromRoute] string id, [FromBody] UpdateTopicDto topic)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            ValidationResult validationResult = await _updateTopicValidator.ValidateAsync(topic);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(this.ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Topic? existingTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            if (existingTopic == null)
            {
                return NotFound();
            }

            existingTopic.Name = topic.Name?.Trim() ?? existingTopic.Name;

            await _context.SaveChangesAsync();
            return Ok(existingTopic.ToDto());
        }

        [HttpDelete("{id}", Name = "DestroyTopic")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetTopicDto>> DestroyAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            Topic? topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null)
            {
                return NotFound();
            }

            _context.Topics.Remove(topic);

            return await _context.SaveChangesAsync() > 0 ? Ok(topic.ToDto()) : NotFound();
        }
    }
}
