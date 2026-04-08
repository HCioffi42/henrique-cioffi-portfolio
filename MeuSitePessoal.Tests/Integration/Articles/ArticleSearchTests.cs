using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// HC: Integration tests for the advanced search functionality.
/// Validates endpoint routing, parameter binding, and database filtering rules.
/// </summary>
public class ArticleSearchTests : BaseIntegrationTest
{
    [Fact]
    public async Task Search_WithTermInTitle_ShouldReturnMatchingArticles()
    {
        // Arrange
        // Seeds the database with known articles to ensure predictable search results.
        await AuthenticateAsync();
        var command1 = new CreateArticleCommand("Unique Search Title", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        var command2 = new CreateArticleCommand("Another Article", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        await _client.PostAsJsonAsync("/api/articles", command1);
        await _client.PostAsJsonAsync("/api/articles", command2);

        // Act
        // Executes the search query against the API endpoint.
        var response = await _client.GetAsync("/api/articles/search?searchTerm=Unique");

        // Assert
        // Verifies that only the article containing the specific term in its title is returned.
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
        // Ensures the search logic extends to the summary field, not just the title.
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Contains(result.Items, a => a.Summary.Contains("SecretKeyword"));
    }

    [Fact]
    public async Task Search_WithCaseInsensitiveTerm_ShouldReturnMatchingArticles()
    {
        // Arrange
        await AuthenticateAsync();
        var command = new CreateArticleCommand("UPPERCASE TITLE", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        await _client.PostAsJsonAsync("/api/articles", command);

        // Act
        // Searches using lowercase characters to test database collation or application-level normalization.
        var response = await _client.GetAsync("/api/articles/search?searchTerm=uppercase");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Contains(result.Items, a => a.Title == "UPPERCASE TITLE");
    }

    [Fact]
    public async Task Search_WithPagination_ShouldApplySkipAndTake()
    {
        // Arrange
        // Seeds multiple articles to create a dataset large enough for pagination.
        await AuthenticateAsync();
        for (int i = 1; i <= 5; i++)
        {
            var command = new CreateArticleCommand($"Paginated Match {i}", "Content", "Summary", ArticleCategory.Technology, new List<string>());
            await _client.PostAsJsonAsync("/api/articles", command);
        }

        // Act
        // Requests the second page of results with a specific page size.
        var response = await _client.GetAsync("/api/articles/search?searchTerm=Paginated Match&pageNumber=2&pageSize=2");

        // Assert
        // Validates that the metadata and the returned item count correctly reflect the requested page.
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count); // It should return exactly 2 items for the second page.
        Assert.True(result.TotalCount >= 5); // Ensure it counts the total seeded items.
    }

    [Fact]
    public async Task Search_WithNoMatches_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/articles/search?searchTerm=NonExistentTermXYZ");

        // Assert
        // Confirms that the API responds gracefully with an empty list instead of a 404 or 500 error.
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Search_WithEmptyTerm_ShouldReturnAllArticles()
    {
        // Arrange
        await AuthenticateAsync();
        var command = new CreateArticleCommand("Standard Article", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        await _client.PostAsJsonAsync("/api/articles", command);

        // Act
        var response = await _client.GetAsync("/api/articles/search?searchTerm=");

        // Assert
        // Should behave like a normal list (or return default paged items) if the term is empty.
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }
}