using EasyAnonymousForum.Data;
using EasyAnonymousForum.Server.Features.Queries;
using EasyAnonymousForum.Server.Features.Threads.DTOs;
using EasyAnonymousForum.Server.Features.Topics.DTOs;
using EasyAnonymousForum.Server.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace EasyAnonymousForum.Server.Features.Threads
{
    [ApiController]
    [Route("api/threads")]
    public class ThreadController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IValidator<CreateThreadDto> _createThreadValidator;
        private readonly IValidator<UpdateThreadDto> _updateThreadValidator;
        private readonly IValidator<QueryObject> _queryObjectValidator;

        public ThreadController(DataContext context,
            IValidator<CreateThreadDto> createThreadValidator,
            IValidator<UpdateThreadDto> updateThreadValidator,
            IValidator<QueryObject> queryObjectValidator)
        {
            this._context = context;
            this._createThreadValidator = createThreadValidator;
            this._updateThreadValidator = updateThreadValidator;
            this._queryObjectValidator = queryObjectValidator;
        }

        [HttpGet(Name = "IndexThreads")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetManyThreadDto>> IndexAsync([FromQuery] QueryObject? queryObject)
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

            var threads = _context.Threads.AsQueryable();

            threads = threads.OrderBy(t => t.DatePosted);

            if (!string.IsNullOrWhiteSpace(queryObject?.Query))
            {
                threads = threads.Where(t => EF.Functions.ToTsVector("english", t.Title).Matches(EF.Functions.ToTsQuery("english", $"'{queryObject.Query}':*")));
            }

            int pageNumber = queryObject?.PageNumber ?? 1;
            int pageSize = queryObject?.PageSize ?? 10;

            return Ok(new GetManyThreadDto(
                threads: (await threads.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.Topic)
                .ToListAsync())
                .Select(t => t.ToDto()),
                pageNumber: pageNumber,
                pageSize: pageSize,
                itemCount: await threads.CountAsync()
            ));
        }

        [HttpPost(Name = "CreateThread")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetThreadDto>> CreateAsync([FromBody] CreateThreadDto thread)
        {
            ValidationResult validationResult = await _createThreadValidator.ValidateAsync(thread);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(this.ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ForumThread newThread = new()
            {
                #nullable disable
                Id = null,
                #nullable restore
                Title = thread.Title?.Trim() ?? "",
                Topic = thread.TopicId != null ? await _context.Topics.FirstOrDefaultAsync(t => t.Id == thread.TopicId) : null,
                Body = thread.Body ?? ""
            };

            await _context.Threads.AddAsync(newThread);
            ForumThread? response = await _context.SaveChangesAsync() > 0 ? newThread : null;
            return response == null ? BadRequest(ModelState) : CreatedAtAction(nameof(ShowAsync), new { id = response.Id }, response.ToDto());
        }

        [HttpGet("{id}", Name = "ShowThread")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetTopicDto>> ShowAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            ForumThread? thread = await _context.Threads.Include(t => t.Topic).FirstOrDefaultAsync(t => t.Id == id);

            return thread == null ? NotFound() : Ok(thread.ToDto());
        }

        [HttpPut("{id}", Name = "UpdateThread")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetThreadDto>> UpdateAsync([FromRoute] string id, [FromBody] UpdateThreadDto thread)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            ValidationResult validationResult = await _updateThreadValidator.ValidateAsync(thread);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(this.ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ForumThread? existingThread = await _context.Threads.Include(t => t.Topic).FirstOrDefaultAsync(t => t.Id == id);

            if (existingThread == null)
            {
                return NotFound();
            }

            existingThread.Title = thread.Title?.Trim() ?? existingThread.Title;
            existingThread.Topic = thread.TopicId != null ? await _context.Topics.FirstOrDefaultAsync(t => t.Id == thread.TopicId) : null;
            existingThread.Body = thread.Body ?? existingThread.Body;

            await _context.SaveChangesAsync();
            return Ok(existingThread.ToDto());
        }

        [HttpDelete("{id}", Name = "DestroyThread")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetThreadDto>> DestroyAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            ForumThread? thread = await _context.Threads.Include(t => t.Topic).FirstOrDefaultAsync(t => t.Id == id);

            if (thread == null)
            {
                return NotFound();
            }

            _context.Threads.Remove(thread);

            return await _context.SaveChangesAsync() > 0 ? Ok(thread.ToDto()) : NotFound();
        }
    }
}
