using MediatR;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Application.Articles.Queries.GetRelatedArticles;

/// <summary>
/// HC: Handler that implements the logic to find related articles based on tag intersection.
/// </summary>
public class GetRelatedArticlesQueryHandler : IRequestHandler<GetRelatedArticlesQuery, List<ArticleSummaryDto>>
{
    private readonly BlogDbContext _context;
    private readonly IMemoryCache _cache;

    public GetRelatedArticlesQueryHandler(BlogDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// Processes the query by identifying articles sharing tags with the base article, sorted by intersection count.
    /// </summary>
    public async Task<List<ArticleSummaryDto>> Handle(GetRelatedArticlesQuery request, CancellationToken cancellationToken)
    {
        var cacheVersion = _cache.GetOrCreate<Guid>("Articles_CacheVersion", _ => Guid.NewGuid());
        var cacheKey = $"Related_{request.ArticleId}_l{request.Limit}_v{cacheVersion}";

        if (_cache.TryGetValue(cacheKey, out List<ArticleSummaryDto>? cachedList))
        {
            return cachedList!;
        }
        
        // 1. Get the base article's tags.
        var baseArticle = await _context.Articles
            .AsNoTracking()
            .Select(a => new { a.Id, a.Tags })
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (baseArticle == null || !baseArticle.Tags.Any())
            return new List<ArticleSummaryDto>();

        var baseTags = baseArticle.Tags.Select(t => t.ToLower()).ToList();

        // 2. Fetch all other articles and calculate the intersection in memory (EF Core limitation with complex array logic).
        // Since the blog dataset is relatively small, this is acceptable. For larger datasets, a more direct SQL approach or FTS would be needed.
        var otherArticles = await _context.Articles
            .AsNoTracking()
            .Where(a => a.Id != request.ArticleId)
            .ToListAsync(cancellationToken);

        var related = otherArticles
            .Select(a => new 
            { 
                Article = a, 
                IntersectionCount = a.Tags.Select(t => t.ToLower()).Intersect(baseTags).Count() 
            })
            .Where(x => x.IntersectionCount > 0)
            .OrderByDescending(x => x.IntersectionCount)
            .ThenByDescending(x => x.Article.CreatedAt)
            .Take(request.Limit)
            .Select(x => new ArticleSummaryDto
            {
                Id = x.Article.Id,
                Title = x.Article.Title,
                Summary = x.Article.Summary,
                CreatedAt = x.Article.CreatedAt,
                Tags = x.Article.Tags,
                Category = x.Article.Category
            })
            .ToList();

        _cache.Set(cacheKey, related, TimeSpan.FromMinutes(10));

        return related;
    }
}
