using MediatR;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticlesSearch;

/// <summary>
/// HC: Handles the search for articles using PostgreSQL Full-Text Search or a fallback logic.
/// </summary>
public class GetArticlesSearchQueryHandler : IRequestHandler<GetArticlesSearchQuery, PagedResult<ArticleSummaryDto>>
{
    private readonly IArticleSearchService _searchService;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the handler with the injected database context.
    /// </summary>
    public GetArticlesSearchQueryHandler(IArticleSearchService searchService, IMemoryCache cache)
    {
        _searchService = searchService;
        _cache = cache;
    }

    /// <summary>
    /// Processes the search query by filtering articles based on the search term.
    /// </summary>
    public async Task<PagedResult<ArticleSummaryDto>> Handle(GetArticlesSearchQuery request, CancellationToken cancellationToken)
    {
        var cacheVersion = _cache.GetOrCreate<Guid>("Articles_CacheVersion", _ => Guid.NewGuid());
        var cacheKey = GenerateCacheKey(request, cacheVersion);

        if (_cache.TryGetValue(cacheKey, out PagedResult<ArticleSummaryDto>? cachedResult))
        {
            return cachedResult!;
        }

        // HC: Delegating the search complexity to the specialized service.
        var result = await _searchService.SearchAsync(
            request.SearchTerm, 
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }
    
    private string GenerateCacheKey(GetArticlesSearchQuery query, Guid version)
    {
        // HC: Normalizes the search term to ensure cache consistency.
        var term = query.SearchTerm?.Trim().ToLower() ?? "empty";
        return $"Search_{term}_p{query.PageNumber}_s{query.PageSize}_v{version}";
    }
}
