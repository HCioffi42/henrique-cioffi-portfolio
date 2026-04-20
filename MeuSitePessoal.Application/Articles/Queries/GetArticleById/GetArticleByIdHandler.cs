using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticleById;

/// <summary>
/// Handles the retrieval of a single article by its identifier, providing localized content.
/// </summary>
public class GetArticleByIdHandler : IRequestHandler<GetArticleByIdQuery, ArticleResponse?>
{
    private readonly IArticleRepository _repository;
    private readonly ILanguageProvider _languageProvider;

    public GetArticleByIdHandler(IArticleRepository repository, ILanguageProvider languageProvider)
    {
        _repository = repository;
        _languageProvider = languageProvider;
    }

    public async Task<ArticleResponse?> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
    {
        var article = await _repository.GetByIdAsync(request.Id);
        
        if (article == null) return null;

        var language = _languageProvider.GetCurrentLanguage();

        // Projects the domain entity to a language-agnostic DTO based on the detected language.
        // Also includes raw localized fields to support Administrative UI (Edit).
        return new ArticleResponse(
            article.Id,
            language.StartsWith("pt") ? article.TitlePt : article.TitleEn,
            article.TitleEn,
            article.TitlePt,
            language.StartsWith("pt") ? article.ContentPt : article.ContentEn,
            article.ContentEn,
            article.ContentPt,
            language.StartsWith("pt") ? article.SummaryPt : article.SummaryEn,
            article.SummaryEn,
            article.SummaryPt,
            article.CreatedAt,
            article.Tags,
            article.Category
        );
    }
}
