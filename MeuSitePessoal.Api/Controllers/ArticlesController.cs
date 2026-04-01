using MeuSitePessoal.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Application.Articles.Commands.DeleteArtigo;
using MeuSitePessoal.Application.Articles.Queries.GetArticleById;
using MeuSitePessoal.Application.Articles.Queries.GetAllArticles;
using MeuSitePessoal.Application.Articles.Queries.GetArticlesSearch;
using MeuSitePessoal.Application.Articles.Queries.GetRelatedArticles;
using Microsoft.AspNetCore.Authorization;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Api.Controllers;

/// <summary>
/// Manages HTTP requests related to blog articles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the controller with the MediatR instance.
    /// </summary>
    public ArticlesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Searches for articles using a search term.
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string? searchTerm = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetArticlesSearchQuery(searchTerm, pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a list of related articles for a specific article based on shared tags.
    /// </summary>
    [HttpGet("{id}/related")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRelated(Guid id, [FromQuery] int limit = 4)
    {
        var result = await _mediator.Send(new GetRelatedArticlesQuery(id, limit));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of all articles including their full content.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListarTodos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        // Passes pagination parameters from the URL query string to the MediatR query.
        var query = new GetAllArticlesQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of article summaries, optionally filtered by multiple tags and category.
    /// </summary>
    [HttpGet("summaries")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSummaries(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] List<string>? tags = null,
        [FromQuery] ArticleCategory? category = null)
    {
        var result = await _mediator.Send(new Application.Articles.Queries.GetArticles.GetArticlesQuery(pageNumber, pageSize, tags, category));
        return Ok(result);
    }

    /// <summary>
    /// Handles the creation of a new article.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar([FromBody] CreateArticleCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = id }, id);
    }
    
    /// <summary>
    /// Retrieves a single article by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var artigo = await _mediator.Send(new GetArticleByIdQuery(id));

        if (artigo == null)
        {
            return NotFound();
        }

        return Ok(artigo);
    }
    
    /// <summary>
    /// Deletes an article by its unique identifier.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteArticleCommand(id));
    
        // Returns 204 if success, otherwise returns 404 for the integration test.
        return success ? NoContent() : NotFound();
    }
    
    /// <summary>
    /// Updates an existing article using its unique identifier.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleCommand command)
    {
        if (id != command.Id) return BadRequest();

        var success = await _mediator.Send(command);
    
        // Validates the success of the update operation to return the correct HTTP status.
        return success ? NoContent() : NotFound();
    }
}