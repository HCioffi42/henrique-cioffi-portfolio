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
    private readonly ILanguageProvider _languageProvider;

    public ArticleSearchService(BlogDbContext context, ILanguageProvider languageProvider)
    {
        _context = context;
        _languageProvider = languageProvider;
    }

    public async Task<PagedResult<ArticleSummaryDto>> SearchAsync(
        string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var language = _languageProvider.GetCurrentLanguage();
        IQueryable<Article> query = _context.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            if (_context.Database.IsNpgsql())
            {
                query = query.Where(a => 
                    (EF.Functions.ToTsVector("english", a.TitleEn + " " + a.SummaryEn).Concat(
                        EF.Functions.ToTsVector("portuguese", a.TitlePt + " " + a.SummaryPt)))
                    .Matches(EF.Functions.WebSearchToTsQuery("english", searchTerm)));
            }
            else
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(a => 
                    a.TitleEn.ToLower().Contains(term) || a.SummaryEn.ToLower().Contains(term) ||
                    a.TitlePt.ToLower().Contains(term) || a.SummaryPt.ToLower().Contains(term));
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
                // HC: REMOVED legacy fallback for search.
                Title = language.StartsWith("pt") ? a.TitlePt : a.TitleEn,
                Summary = language.StartsWith("pt") ? a.SummaryPt : a.SummaryEn,
                CreatedAt = a.CreatedAt,
                Tags = a.Tags,
                Category = a.Category
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleSummaryDto>(items, totalCount, pageNumber, pageSize);
    }
}