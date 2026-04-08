using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// High-level abstraction for article searching. 
/// The implementation details (FTS, SQL, etc.) are hidden in the Infrastructure layer.
/// </summary>
public interface IArticleSearchService
{
    Task<PagedResult<ArticleSummaryDto>> SearchAsync(
        string? searchTerm, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken);
}