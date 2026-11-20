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

    /// <summary>
    /// Determines whether the supplied language code selects Portuguese content.
    /// The comparison ignores case and region so that 'pt', 'pt-BR' and 'PT_br' all resolve
    /// to Portuguese rather than falling through to the English default.
    /// </summary>
    public static bool IsPortuguese(string? language) =>
        language?.StartsWith("pt", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Applies the PostgreSQL full-text predicate for a single language. The vector and the
    /// query parser share one dictionary so the search term is stemmed by the same rules as
    /// the text it is matched against, and the expression is kept free of coalesce calls so
    /// that it matches the corresponding GIN index exactly.
    /// </summary>
    public static IQueryable<Article> ApplyFullTextFilter(
        IQueryable<Article> query, string searchTerm, bool isPortuguese) =>
        isPortuguese
            ? query.Where(a => EF.Functions
                .ToTsVector("portuguese", a.TitlePt + " " + a.SummaryPt)
                .Matches(EF.Functions.WebSearchToTsQuery("portuguese", searchTerm)))
            : query.Where(a => EF.Functions
                .ToTsVector("english", a.TitleEn + " " + a.SummaryEn)
                .Matches(EF.Functions.WebSearchToTsQuery("english", searchTerm)));

    public async Task<PagedResult<ArticleSummaryDto>> SearchAsync(
        string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var language = _languageProvider.GetCurrentLanguage();
        var isPortuguese = IsPortuguese(language);
        IQueryable<Article> query = _context.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            if (_context.Database.IsNpgsql())
            {
                query = ApplyFullTextFilter(query, searchTerm, isPortuguese);
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
                Title = isPortuguese ? a.TitlePt : a.TitleEn,
                Summary = isPortuguese ? a.SummaryPt : a.SummaryEn,
                CreatedAt = a.CreatedAt,
                Tags = a.Tags,
                Category = a.Category
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleSummaryDto>(items, totalCount, pageNumber, pageSize);
    }
}