using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Application.Comments.Commands.CreateComment;
using MeuSitePessoal.Application.Comments.Queries.GetCommentsByArticleId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuSitePessoal.Api.Controllers;

/// <summary>
/// Manages HTTP requests related to the nested comment system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the controller with the MediatR instance.
    /// </summary>
    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a nested tree of comments for a specific article.
    /// </summary>
    /// <param name="articleId">The unique identifier of the article.</param>
    [HttpGet("/api/articles/{articleId:guid}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByArticle(Guid articleId)
    {
        var result = await _mediator.Send(new GetCommentsByArticleIdQuery(articleId));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Type == ErrorType.NotFound)
        {
            return NotFound(new { message = result.Error });
        }

        return BadRequest(new { message = result.Error });
    }

    /// <summary>
    /// Creates a new comment or reply.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateCommentCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            // We return 201 Created and the ID of the new comment.
            return CreatedAtAction(nameof(GetByArticle), new { articleId = command.ArticleId }, new { id = result.Value });
        }

        if (result.Type == ErrorType.NotFound)
        {
            return NotFound(new { message = result.Error });
        }

        if (result.Type == ErrorType.Conflict)
        {
            return Conflict(new { message = result.Error });
        }

        return BadRequest(new { message = result.Error });
    }
}
