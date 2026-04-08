using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Domain.Interfaces;

// Defines the contract for the Article repository, providing methods for complete data persistence.
public interface IArticleRepository
{
    // Fetches a paginated list of articles and the total count.
    Task<(IEnumerable<Article> Items, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize);
    
    // Retrieves a specific article by its unique identifier. Returns null if no match is found.
    Task<Article?> GetByIdAsync(Guid id);
    
    // Persists a new article entity into the data store.
    Task AddAsync(Article article);
    
    // Updates the properties of an existing article. Returns true if the record was successfully updated.
    Task<bool> UpdateAsync(Article article);

    // Removes an article from the database based on its unique ID. Returns true if the deletion succeeded.
    Task<bool> DeleteAsync(Guid id);
}