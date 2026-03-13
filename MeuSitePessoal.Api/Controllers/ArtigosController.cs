using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MeuSitePessoal.Application.Commands;

namespace MeuSitePessoal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtigosController : ControllerBase
{
    private readonly IArtigoRepository _repository;
    private readonly IMediator _mediator;

    // Recebo o repositório e o mediator via injeção de dependência.
    public ArtigosController(IArtigoRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        // Solicita ao repositório a lista completa de artigos cadastrados no banco.
        var artigos = await _repository.ObterTodosAsync();
        return Ok(artigos);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateArtigoCommand command)
    {
        // Envia o comando para o handler via MediatR.
        var id = await _mediator.Send(command);
        
        // Retorna o status 201 Created com o ID do novo artigo.
        return CreatedAtAction(nameof(ObterPorId), new { id = id }, id);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        // Requests a specific article from the repository using the provided ID.
        var artigo = await _repository.ObterPorIdAsync(id);

        // Returns a 404 Not Found response if the article does not exist.
        if (artigo == null)
        {
            return NotFound();
        }

        // Returns the found article with a 200 OK status.
        return Ok(artigo);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        // Calls the repository to delete the record and checks if the operation succeeded.
        var excluido = await _repository.ExcluirAsync(id);

        if (!excluido)
        {
            return NotFound();
        }

        // Returns 204 No Content to indicate successful deletion without a response body.
        return NoContent();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] Artigo artigo)
    {
        // Ensures the ID in the URL matches the ID in the request body.
        if (id != artigo.Id)
        {
            return BadRequest("ID mismatch.");
        }

        // Calls the repository to perform the update operation.
        var atualizado = await _repository.AtualizarAsync(artigo);

        if (!atualizado)
        {
            return NotFound();
        }

        // Returns 204 No Content to confirm the update was successful.
        return NoContent();
    }
}