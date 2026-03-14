using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Domain.Interfaces;

// Defines the contract for the Artigo repository, providing methods for complete data persistence.
public interface IArtigoRepository
{
    // Fetches a paginated list of articles and the total count.
    Task<(IEnumerable<Artigo> Items, int TotalCount)> ObterPaginadoAsync(int pageNumber, int pageSize);
    
    // Retrieves a specific article by its unique identifier. Returns null if no match is found.
    Task<Artigo?> ObterPorIdAsync(Guid id);
    
    // Persists a new article entity into the data store.
    Task AdicionarAsync(Artigo artigo);
    
    // Updates the properties of an existing article. Returns true if the record was successfully updated.
    Task<bool> AtualizarAsync(Artigo artigo);

    // Removes an article from the database based on its unique ID. Returns true if the deletion succeeded.
    Task<bool> ExcluirAsync(Guid id);
}