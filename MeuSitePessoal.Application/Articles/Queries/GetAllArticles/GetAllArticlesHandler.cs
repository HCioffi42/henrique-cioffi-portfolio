using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Common.Interfaces;

namespace MeuSitePessoal.Application.Articles.Queries.GetAllArticles;

/// <summary>
/// Handles the retrieval of all articles for the administrative dashboard.
/// Projects items to ArticleResponse to ensure localized titles are available.
/// </summary>
public class GetAllArticlesHandler : IRequestHandler<GetAllArticlesQuery, PagedList<ArticleResponse>>
{
    private readonly IArticleRepository _repository;
    private readonly ILanguageProvider _languageProvider;

    public GetAllArticlesHandler(IArticleRepository repository, ILanguageProvider languageProvider)
    {
        _repository = repository;
        _languageProvider = languageProvider;
    }

    public async Task<PagedList<ArticleResponse>> Handle(GetAllArticlesQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch raw entities from repository
        var (items, totalCount) = await _repository.GetPaginatedAsync(request.PageNumber, request.PageSize);
        
        var language = _languageProvider.GetCurrentLanguage();

        // 2. Project to ArticleResponse, resolving the active language from the localized
        // columns. Both variants are carried alongside so the dashboard can edit either one.
        var isPortuguese = language.StartsWith("pt", StringComparison.OrdinalIgnoreCase);

        var projectedItems = items.Select(article => new ArticleResponse(
            article.Id,
            isPortuguese ? article.TitlePt : article.TitleEn,
            article.TitleEn,
            article.TitlePt,
            isPortuguese ? article.ContentPt : article.ContentEn,
            article.ContentEn,
            article.ContentPt,
            isPortuguese ? article.SummaryPt : article.SummaryEn,
            article.SummaryEn,
            article.SummaryPt,
            article.CreatedAt,
            article.Tags,
            article.Category
        )).ToList();

        // 3. Return the projected list
        return new PagedList<ArticleResponse>(
            projectedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
