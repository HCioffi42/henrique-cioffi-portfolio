using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuSitePessoal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtigosController : ControllerBase
{
    private readonly IArtigoRepository _repository;

    // Recebo o repositório via injeção de dependência para isolar a lógica de dados.
    public ArtigosController(IArtigoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        // Solicita ao repositório a lista completa de artigos cadastrados no banco.
        var artigos = await _repository.ObterTodosAsync();
        return Ok(artigos);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(Artigo artigo)
    {
        // Define a data de criação no momento da inserção antes de enviar para o repositório.
        artigo.DataCriacao = DateTime.UtcNow;
        
        await _repository.AdicionarAsync(artigo);
        
        // Retorna o status 200 OK para confirmar que o registro foi salvo com sucesso.
        return Ok(artigo);
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
}