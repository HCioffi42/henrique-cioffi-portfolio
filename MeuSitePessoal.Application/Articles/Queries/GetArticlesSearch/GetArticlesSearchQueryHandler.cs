using MediatR;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticlesSearch;

/// <summary>
/// HC: Handles the search for articles using PostgreSQL Full-Text Search or a fallback logic.
/// </summary>
public class GetArticlesSearchQueryHandler : IRequestHandler<GetArticlesSearchQuery, PagedResult<ArticleSummaryDto>>
{
    private readonly BlogDbContext _context;

    /// <summary>
    /// Initializes a new instance of the handler with the injected database context.
    /// </summary>
    public GetArticlesSearchQueryHandler(BlogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Processes the search query by filtering articles based on the search term.
    /// </summary>
    public async Task<PagedResult<ArticleSummaryDto>> Handle(GetArticlesSearchQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // HC: Checks if we are running against a PostgreSQL provider.
            if (_context.Database.IsNpgsql())
            {
                // HC: Use the high-performance bilingual FTS engine.
                query = query.Where(a => 
                    (EF.Functions.ToTsVector("english", a.Title + " " + a.Summary).Concat(
                        EF.Functions.ToTsVector("portuguese", a.Title + " " + a.Summary)))
                    .Matches(EF.Functions.WebSearchToTsQuery("english", request.SearchTerm)));
            }
            else
            {
                // HC: Fallback for In-Memory tests or other providers.
                // This allows unit tests to pass while maintaining basic logic validation.
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(a => 
                    a.Title.ToLower().Contains(searchTerm) || 
                    a.Summary.ToLower().Contains(searchTerm));
            }               
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArticleSummaryDto
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                CreatedAt = a.CreatedAt,
                Tags = a.Tags,
                Category = a.Category
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
