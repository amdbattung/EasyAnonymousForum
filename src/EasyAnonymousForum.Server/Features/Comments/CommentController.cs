using EasyAnonymousForum.Data;
using EasyAnonymousForum.Server.Features.Comments.DTOs;
using EasyAnonymousForum.Server.Features.Queries;
using EasyAnonymousForum.Server.Features.Threads.DTOs;
using EasyAnonymousForum.Server.Features.Threads.Validators;
using EasyAnonymousForum.Server.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Threading;

namespace EasyAnonymousForum.Server.Features.Comments
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IValidator<CreateCommentDto> _createCommentValidator;
        private readonly IValidator<UpdateCommentDto> _updateCommentValidator;
        private readonly IValidator<QueryObject> _queryObjectValidator;

        public CommentController(DataContext context,
            IValidator<CreateCommentDto> createCommentValidator,
            IValidator<UpdateCommentDto> updateCommentValidator,
            IValidator<QueryObject> queryObjectValidator)
        {
            this._context = context;
            this._createCommentValidator = createCommentValidator;
            this._updateCommentValidator = updateCommentValidator;
            this._queryObjectValidator = queryObjectValidator;
        }

        [HttpGet(Name = "IndexComments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetManyCommentDto>> IndexAsync([FromQuery] QueryObject? queryObject)
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

            var comments = _context.Comments.AsQueryable();

            comments = comments.OrderBy(t => t.DatePosted);

            if (!string.IsNullOrWhiteSpace(queryObject?.Query))
            {
                comments = comments.Where(c => EF.Functions.ToTsVector("simple", c.Content).Matches(EF.Functions.ToTsQuery("simple", $"'{queryObject.Query}':*")));
            }

            int pageNumber = queryObject?.PageNumber ?? 1;
            int pageSize = queryObject?.PageSize ?? 10;

            return Ok(new GetManyCommentDto(
                comments: (await comments.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Thread)
                .ToListAsync())
                .Select(c => c.ToDto()),
                pageNumber: pageNumber,
                pageSize: pageSize,
                itemCount: await comments.CountAsync()
            ));
        }

        [HttpPost(Name = "CreateComment")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetCommentDto>> CreateAsync([FromBody] CreateCommentDto comment)
        {
            ValidationResult validationResult = await _createCommentValidator.ValidateAsync(comment);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(this.ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ForumThread? thread = await _context.Threads.FirstOrDefaultAsync(t => t.Id == comment.ThreadId);

            if (thread == null)
            {
                return BadRequest();
            }

            Comment newComment = new()
            {
                #nullable disable
                Id = null,
                #nullable restore
                Thread = thread,
                Content = comment.Content ?? ""
            };

            await _context.Comments.AddAsync(newComment);
            Comment? response = await _context.SaveChangesAsync() > 0 ? newComment : null;
            return response == null ? BadRequest(ModelState) : CreatedAtAction(nameof(ShowAsync), new { id = response.Id }, response.ToDto());
        }

        [HttpGet("{id}", Name = "ShowComment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCommentDto>> ShowAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            Comment? comment = await _context.Comments.Include(c => c.Thread).FirstOrDefaultAsync(t => t.Id == id);

            return comment == null ? NotFound() : Ok(comment.ToDto());
        }

        [HttpPut("{id}", Name = "UpdateComment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCommentDto>> UpdateAsync([FromRoute] string id, [FromBody] UpdateCommentDto comment)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            ValidationResult validationResult = await _updateCommentValidator.ValidateAsync(comment);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(this.ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Comment? existingComment = await _context.Comments.Include(c => c.Thread).FirstOrDefaultAsync(c => c.Id == id);

            if (existingComment == null)
            {
                return NotFound();
            }

            existingComment.Thread = !string.IsNullOrWhiteSpace(comment.ThreadId) ? await _context.Threads.FirstOrDefaultAsync(t => t.Id == comment.ThreadId) ?? existingComment.Thread : existingComment.Thread;
            existingComment.Content = comment.Content ?? existingComment.Content;

            await _context.SaveChangesAsync();
            return Ok(existingComment.ToDto());
        }

        [HttpDelete("{id}", Name = "DestroyComment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCommentDto>> DestroyAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Null or invalid id");
            }

            Comment? comment = await _context.Comments.Include(t => t.Thread).FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);

            return await _context.SaveChangesAsync() > 0 ? Ok(comment.ToDto()) : NotFound();
        }
    }
}
