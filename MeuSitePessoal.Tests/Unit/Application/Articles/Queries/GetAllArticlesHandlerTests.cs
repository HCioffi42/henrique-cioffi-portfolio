using FluentAssertions;
using MeuSitePessoal.Application.Articles.Queries.GetAllArticles;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Queries;

/// <summary>
/// Covers the administrative listing projection after the legacy single-language
/// columns were retired. The handler resolves the active language directly from the
/// localized columns and carries no fallback.
/// </summary>
public class GetAllArticlesHandlerTests
{
    private readonly Mock<IArticleRepository> _repositoryMock = new();
    private readonly Mock<ILanguageProvider> _languageProviderMock = new();

    private GetAllArticlesHandler CreateHandler() =>
        new(_repositoryMock.Object, _languageProviderMock.Object);

    private void GivenArticles(params Article[] articles) =>
        _repositoryMock
            .Setup(x => x.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((articles.AsEnumerable(), articles.Length));

    private static Article AnArticle() =>
        new("Hello", "Olá", "Body", "Corpo", "Summary", "Resumo",
            new List<string>(), ArticleCategory.Technology);

    [Fact]
    public async Task Handle_ResolvesPortugueseFields_WhenLanguageIsPt()
    {
        GivenArticles(AnArticle());
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("pt-BR");

        var result = await CreateHandler().Handle(new GetAllArticlesQuery(1, 10), CancellationToken.None);

        var item = result.Items.Single();
        item.Title.Should().Be("Olá");
        item.Content.Should().Be("Corpo");
        item.Summary.Should().Be("Resumo");
    }

    [Fact]
    public async Task Handle_ResolvesEnglishFields_WhenLanguageIsEn()
    {
        GivenArticles(AnArticle());
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("en");

        var result = await CreateHandler().Handle(new GetAllArticlesQuery(1, 10), CancellationToken.None);

        var item = result.Items.Single();
        item.Title.Should().Be("Hello");
        item.Content.Should().Be("Body");
        item.Summary.Should().Be("Summary");
    }

    [Fact]
    public async Task Handle_AlwaysExposesBothLocalizedVariants()
    {
        GivenArticles(AnArticle());
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("en");

        var result = await CreateHandler().Handle(new GetAllArticlesQuery(1, 10), CancellationToken.None);

        var item = result.Items.Single();
        item.TitleEn.Should().Be("Hello");
        item.TitlePt.Should().Be("Olá");
        item.SummaryEn.Should().Be("Summary");
        item.SummaryPt.Should().Be("Resumo");
    }

    [Fact]
    public async Task Handle_WithIdenticalLocalizedValues_ReturnsThatValue()
    {
        // The shape every article migrated from the single-language schema carries:
        // the original text was copied into both localized columns.
        var article = new Article("Same", "Same", "Body", "Body", "Summary", "Summary",
            new List<string>(), ArticleCategory.Technology);
        GivenArticles(article);
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("pt");

        var result = await CreateHandler().Handle(new GetAllArticlesQuery(1, 10), CancellationToken.None);

        result.Items.Single().Title.Should().Be("Same");
    }
}
