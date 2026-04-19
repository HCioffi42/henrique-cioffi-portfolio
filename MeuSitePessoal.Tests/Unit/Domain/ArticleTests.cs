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
        var expectedTitle = "TDD in Practice";
        var expectedContent = "Content about unit testing.";
        var expectedSummary = "A practical guide to TDD.";
        var expectedTags = new List<string> { "tdd", "dotnet" };

        // Act
        var article = new Article(expectedTitle, expectedContent, expectedSummary, expectedTags, ArticleCategory.Technology);

        // Assert
        Assert.NotNull(article);
        Assert.Equal(expectedTitle, article.Title);
        Assert.Equal(expectedSummary, article.Summary);
        Assert.Equal(expectedTags, article.Tags);
        Assert.Equal(ArticleCategory.Technology, article.Category);
        
        // HC: Verify "Split Brain" synchronization
        Assert.Equal(expectedTitle, article.TitleEn);
        Assert.Equal(expectedTitle, article.TitlePt);
        Assert.Equal(expectedContent, article.ContentEn);
        Assert.Equal(expectedContent, article.ContentPt);
        Assert.Equal(expectedSummary, article.SummaryEn);
        Assert.Equal(expectedSummary, article.SummaryPt);
    }

    [Fact]
    public void CreateArticle_WithSpecificCategory_ShouldAssignCorrectly()
    {
        // Arrange
        var category = ArticleCategory.Tutorial;

        // Act
        var article = new Article("Title", "Content", "Summary", new List<string>(), category);

        // Assert
        Assert.Equal(category, article.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateArticle_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Article(invalidTitle, "Some content", "Summary", new List<string>(), ArticleCategory.Technology));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateArticle_WithInvalidContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Article("Title", invalidContent, "Summary", new List<string>(), ArticleCategory.Technology));
    }

    [Fact]
    public void UpdateContent_ShouldMaintainSynchronization()
    {
        // Arrange
        var article = new Article("Original", "Content", "Summary", new List<string>(), ArticleCategory.Technology);

        // Act
        article.UpdateContent("en", "Updated Title", "Updated Content", "Updated Summary");

        // Assert
        Assert.Equal("Updated Title", article.Title);
        Assert.Equal("Updated Title", article.TitleEn);
        Assert.Equal("Updated Title", article.TitlePt); // Sync ensures all are updated
    }
}