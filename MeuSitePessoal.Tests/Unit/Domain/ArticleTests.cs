using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Domain;

public class ArticleTests
{
    [Fact]
    public void CreateArticle_WithValidData_ShouldSucceed()
    {
        // Arrange
        var expectedTitleEn = "TDD in Practice";
        var expectedTitlePt = "TDD na Prática";
        var expectedContentEn = "Content about unit testing.";
        var expectedContentPt = "Conteúdo sobre testes unitários.";
        var expectedSummaryEn = "A practical guide to TDD.";
        var expectedSummaryPt = "Um guia prático de TDD.";
        var expectedTags = new List<string> { "tdd", "dotnet" };

        // Act
        var article = new Article(
            expectedTitleEn, expectedTitlePt,
            expectedContentEn, expectedContentPt,
            expectedSummaryEn, expectedSummaryPt,
            expectedTags, ArticleCategory.Technology);

        // Assert
        Assert.NotNull(article);
        Assert.Equal(expectedTitleEn, article.TitleEn);
        Assert.Equal(expectedTitlePt, article.TitlePt);
        Assert.Equal(expectedSummaryEn, article.SummaryEn);
        Assert.Equal(expectedSummaryPt, article.SummaryPt);
        Assert.Equal(expectedTags, article.Tags);
        Assert.Equal(ArticleCategory.Technology, article.Category);
        
        // HC: Legacy fields should be empty for new multi-language constructor.
        Assert.Equal(string.Empty, article.Title);
        Assert.Equal(string.Empty, article.Content);
        Assert.Equal(string.Empty, article.Summary);
    }

    [Fact]
    public void CreateArticle_WithSpecificCategory_ShouldAssignCorrectly()
    {
        // Arrange
        var category = ArticleCategory.Tutorial;

        // Act
        var article = new Article("Title", "Título", "Content", "Conteúdo", "Summary", "Resumo", new List<string>(), category);

        // Assert
        Assert.Equal(category, article.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateArticle_WithInvalidTitleEn_ShouldThrowArgumentException(string invalidTitle)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Article(invalidTitle, "Título", "Some content", "Conteúdo", "Summary", "Resumo", new List<string>(), ArticleCategory.Technology));
    }

    [Fact]
    public void UpdateContent_ShouldMaintainSynchronization()
    {
        // Arrange
        var article = new Article("Original", "Original", "Content", "Conteúdo", "Summary", "Resumo", new List<string>(), ArticleCategory.Technology);

        // Act
        article.UpdateContent("en", "Updated Title", "Updated Content", "Updated Summary");

        // Assert
        Assert.Equal("Updated Title", article.TitleEn);
        Assert.Equal("Original", article.TitlePt); // PT stays original
        Assert.Equal(string.Empty, article.Title); // Legacy stays empty
    }
}