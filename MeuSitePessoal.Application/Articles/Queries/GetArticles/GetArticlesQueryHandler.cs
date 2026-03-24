using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticles;

/// <summary>
/// Handles the retrieval of paginated article summaries and applies multi-tag filtering if requested.
/// </summary>
public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PagedResult<ArticleSummaryDto>>
{
    private readonly BlogDbContext _context;

    /// <summary>
    /// Initializes a new instance of the handler with the injected database context.
    /// </summary>
    public GetArticlesQueryHandler(BlogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Processes the query by filtering, paginating, and projecting the article summaries.
    /// </summary>
    public async Task<PagedResult<ArticleSummaryDto>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Articles.AsNoTracking();

        // Chains multiple Where clauses using LINQ Aggregate to ensure all requested tags are present (AND logic).
        if (request.Tags?.Any() == true)
        {
            query = request.Tags.Aggregate(query, (current, tag) => 
                current.Where(a => a.Tags.Contains(tag)));
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