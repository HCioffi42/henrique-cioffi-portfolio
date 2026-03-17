using MeuSitePessoal.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;
using MeuSitePessoal.Application.Artigos.Commands.DeleteArtigo;
using MeuSitePessoal.Application.Artigos.Queries.GetArtigoById;
using MeuSitePessoal.Application.Artigos.Queries.GetTodosArtigos;
using Microsoft.AspNetCore.Authorization;

namespace MeuSitePessoal.Api.Controllers;

/// <summary>
/// Manages HTTP requests related to blog articles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class ArtigosController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the controller with the MediatR instance.
    /// </summary>
    public ArtigosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a paginated list of all articles including their full content.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListarTodos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        // Passes pagination parameters from the URL query string to the MediatR query.
        var query = new GetTodosArtigosQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of article summaries, optionally filtered by multiple tags.
    /// </summary>
    [HttpGet("summaries")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSummaries([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] List<string>? tags = null)
    {
        var result = await _mediator.Send(new Application.Artigos.Queries.GetArtigos.GetArtigosQuery(pageNumber, pageSize, tags));
        return Ok(result);
    }

    /// <summary>
    /// Handles the creation of a new article.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Criar([FromBody] CreateArtigoCommand command)
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
        var artigo = await _mediator.Send(new GetArtigoByIdQuery(id));

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
    public async Task<IActionResult> Excluir(Guid id)
    {
        await _mediator.Send(new DeleteArtigoCommand(id));
        return NoContent();
    }
    
    /// <summary>
    /// Updates an existing article using its unique identifier.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateArtigoCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch.");
        }

        await _mediator.Send(command);
        return NoContent();
    }
}