using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Domain.Interfaces;

public interface IArtigoRepository
{
    // Fetches all articles from the database asynchronously.
    Task<IEnumerable<Artigo>> ObterTodosAsync();
    
    // Retrieves a specific article by its unique identifier or returns null if not found.
    Task<Artigo?> ObterPorIdAsync(Guid id);
    
    // Persists a new article entity into the database.
    Task AdicionarAsync(Artigo artigo);
    
    // Removes an article from the database based on its unique ID.
    Task<bool> ExcluirAsync(Guid id);
    
    // Updates an existing article in the database.
    Task<bool> AtualizarAsync(Artigo artigo);
}