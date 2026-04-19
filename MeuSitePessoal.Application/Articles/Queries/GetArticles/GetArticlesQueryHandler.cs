using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticles;

/// <summary>
/// Handles the retrieval of paginated article summaries and applies multi-tag filtering if requested.
/// Integrated with ILanguageProvider for localized content.
/// </summary>
public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PagedResult<ArticleSummaryDto>>
{
    private readonly IBlogDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILanguageProvider _languageProvider;
    private const string ArticlesCacheKeyPrefix = "Articles_";

    /// <summary>
    /// Initializes a new instance of the handler with the injected database context, memory cache, and language provider.
    /// </summary>
    public GetArticlesQueryHandler(IBlogDbContext context, IMemoryCache cache, ILanguageProvider languageProvider)
    {
        _context = context;
        _cache = cache;
        _languageProvider = languageProvider;
    }

    /// <summary>
    /// Processes the query by filtering, paginating, and projecting the article summaries, utilizing caching for performance.
    /// </summary>
    public async Task<PagedResult<ArticleSummaryDto>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        var language = _languageProvider.GetCurrentLanguage();
        
        // Get the current cache version to ensure data consistency after invalidation.
        var cacheVersion = _cache.GetOrCreate<Guid>("Articles_CacheVersion", _ => Guid.NewGuid());
        var cacheKey = GenerateCacheKey(request, cacheVersion, language);

        if (_cache.TryGetValue(cacheKey, out PagedResult<ArticleSummaryDto>? cachedResult))
        {
            return cachedResult!;
        }

        // HC: Explicitly typed as IQueryable to avoid the "Ambiguous invocation" error seen before.
        IQueryable<Article> query = _context.Articles.AsNoTracking();

        // Chains multiple Where clauses using LINQ Aggregate to ensure all requested tags are present (AND logic).
        if (request.Tags != null && request.Tags.Any())
        {
            foreach (var tag in request.Tags)
            {
                var targetTag = tag.ToLower();
                query = query.Where(a => a.Tags.Any(t => t == targetTag));
            }
        }

        // Apply Category filter if provided (exact match).
        if (request.Category.HasValue)
        {
            query = query.Where(a => a.Category == request.Category.Value);
        }
    
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArticleSummaryDto
            {
                Id = a.Id,
                Title = language.StartsWith("pt") ? a.TitlePt : a.TitleEn,
                Summary = language.StartsWith("pt") ? a.SummaryPt : a.SummaryEn,
                CreatedAt = a.CreatedAt,
                Tags = a.Tags,
                Category = a.Category
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ArticleSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));

        _cache.Set(cacheKey, result, cacheEntryOptions);

        return result;
    }

    private string GenerateCacheKey(GetArticlesQuery query, object version, string language)
    {
        var tagsPart = query.Tags != null ? string.Join(",", query.Tags.OrderBy(t => t)) : "none";
        return $"{ArticlesCacheKeyPrefix}v{version}_l{language}_p{query.PageNumber}_s{query.PageSize}_c{query.Category ?? 0}_t{tagsPart}";
    }
}
