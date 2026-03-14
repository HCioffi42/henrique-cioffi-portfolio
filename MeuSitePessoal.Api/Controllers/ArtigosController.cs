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

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArtigosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ArtigosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListarTodos()
    {
        var artigos = await _mediator.Send(new GetTodosArtigosQuery());
        return Ok(artigos);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateArtigoCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = id }, id);
    }
    
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
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        await _mediator.Send(new DeleteArtigoCommand(id));
        return NoContent();
    }
    
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