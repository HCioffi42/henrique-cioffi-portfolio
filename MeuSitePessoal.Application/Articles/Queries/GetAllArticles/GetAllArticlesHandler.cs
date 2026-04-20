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

        // 2. Project to ArticleResponse manually to handle the legacy vs localized fallback logic.
        // We prioritize localized columns (En/Pt) and fallback to legacy for older data.
        var projectedItems = items.Select(article => new ArticleResponse(
            article.Id,
            language.StartsWith("pt") 
                ? (!string.IsNullOrEmpty(article.TitlePt) ? article.TitlePt : article.Title)
                : (!string.IsNullOrEmpty(article.TitleEn) ? article.TitleEn : article.Title),
            article.TitleEn,
            article.TitlePt,
            language.StartsWith("pt") 
                ? (!string.IsNullOrEmpty(article.ContentPt) ? article.ContentPt : article.Content)
                : (!string.IsNullOrEmpty(article.ContentEn) ? article.ContentEn : article.Content),
            article.ContentEn,
            article.ContentPt,
            language.StartsWith("pt") 
                ? (!string.IsNullOrEmpty(article.SummaryPt) ? article.SummaryPt : article.Summary)
                : (!string.IsNullOrEmpty(article.SummaryEn) ? article.SummaryEn : article.Summary),
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
