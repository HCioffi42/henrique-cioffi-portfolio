using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Services;

public class ArticleSearchService : IArticleSearchService
{
    private readonly BlogDbContext _context;

    public ArticleSearchService(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArticleSummaryDto>> SearchAsync(
        string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        IQueryable<Article> query = _context.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // HC: Here we have full access to Npgsql specific extensions.
            if (_context.Database.IsNpgsql())
            {
                query = query.Where(a => 
                    (EF.Functions.ToTsVector("english", a.Title + " " + a.Summary).Concat(
                        EF.Functions.ToTsVector("portuguese", a.Title + " " + a.Summary)))
                    .Matches(EF.Functions.WebSearchToTsQuery("english", searchTerm)));
            }
            else
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(term) || a.Summary.ToLower().Contains(term));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        return new PagedResult<ArticleSummaryDto>(items, totalCount, pageNumber, pageSize);
    }
}