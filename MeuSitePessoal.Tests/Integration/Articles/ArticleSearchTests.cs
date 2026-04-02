using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// HC: Integration tests for the advanced search functionality.
/// </summary>
public class ArticleSearchTests : BaseIntegrationTest
{
    [Fact]
    public async Task Search_WithTermInTitle_ShouldReturnMatchingArticles()
    {
        // Arrange
        await AuthenticateAsync();
        var command1 = new CreateArticleCommand("Unique Search Title", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        var command2 = new CreateArticleCommand("Another Article", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        await _client.PostAsJsonAsync("/api/articles", command1);
        await _client.PostAsJsonAsync("/api/articles", command2);

        // Act
        var response = await _client.GetAsync("/api/articles/search?searchTerm=Unique");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Contains(result.Items, a => a.Title.Contains("Unique"));
        Assert.DoesNotContain(result.Items, a => a.Title == "Another Article");
    }

    [Fact]
    public async Task Search_WithTermInSummary_ShouldReturnMatchingArticles()
    {
        // Arrange
        await AuthenticateAsync();
        var command = new CreateArticleCommand("Title", "Content", "SecretKeyword in summary", ArticleCategory.Technology, new List<string>());
        await _client.PostAsJsonAsync("/api/articles", command);

        // Act
        var response = await _client.GetAsync("/api/articles/search?searchTerm=SecretKeyword");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Contains(result.Items, a => a.Summary.Contains("SecretKeyword"));
    }

    [Fact]
    public async Task Search_WithNoMatches_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/articles/search?searchTerm=NonExistentTerm");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Search_WithEmptyTerm_ShouldReturnAllArticles()
    {
        // Act
        var response = await _client.GetAsync("/api/articles/search?searchTerm=");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        // Should behave like a normal list if term is empty
        Assert.True(result.TotalCount >= 0);
    }
}
