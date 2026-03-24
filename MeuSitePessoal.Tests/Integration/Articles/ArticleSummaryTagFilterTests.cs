using System.Net.Http.Json;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Domain;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// Integration tests for validating multi-tag filtering logic on article summaries.
/// </summary>
public class ArticleSummaryTagFilterTests : BaseIntegrationTest
{
    public record ArticleSummaryResponse(Guid Id, string Title, string Summary, DateTime CreatedAt, List<string> Tags, ArticleCategory Category);
    public record PagedSummaryResponse(List<ArticleSummaryResponse> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages);

    [Fact]
    public async Task GetSummaries_WithCategoryFilter_ShouldReturnMatchingArticles()
    {
        // Arrange
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Tech 1", "Content", "Summary", ArticleCategory.Technology, new List<string>()));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("News 1", "Content", "Summary", ArticleCategory.News, new List<string>()));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Tech 2", "Content", "Summary", ArticleCategory.Technology, new List<string>()));

        // Act
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/articles/summaries?category={(int)ArticleCategory.Technology}");
        response.EnsureSuccessStatusCode();

        var pagedResult = await response.Content.ReadFromJsonAsync<PagedSummaryResponse>();

        // Assert
        Assert.NotNull(pagedResult);
        Assert.Equal(2, pagedResult.Items.Count);
        Assert.All(pagedResult.Items, item => Assert.Equal(ArticleCategory.Technology, item.Category));
    }

    [Fact]
    public async Task GetSummaries_WithCategoryAndTags_ShouldReturnIntersection()
    {
        // Arrange
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Tech Dotnet", "Content", "Summary", ArticleCategory.Technology, new List<string> { "dotnet" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Tech React", "Content", "Summary", ArticleCategory.Technology, new List<string> { "react" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("News Dotnet", "Content", "Summary", ArticleCategory.News, new List<string> { "dotnet" }));

        // Act
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/articles/summaries?category={(int)ArticleCategory.Technology}&tags=dotnet");
        response.EnsureSuccessStatusCode();

        var pagedResult = await response.Content.ReadFromJsonAsync<PagedSummaryResponse>();

        // Assert
        Assert.NotNull(pagedResult);
        Assert.Single(pagedResult.Items);
        Assert.Equal("Tech Dotnet", pagedResult.Items[0].Title);
        Assert.Equal(ArticleCategory.Technology, pagedResult.Items[0].Category);
        Assert.Contains("dotnet", pagedResult.Items[0].Tags);
    }

    [Fact]
    public async Task GetSummaries_WithSingleTagFilter_ShouldReturnMatchingArticles()
    {
        // Arrange: Authenticates for setup and seeds data.
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 1", "Content", "Summary", ArticleCategory.Technology, new List<string> { "dotnet", "csharp" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 2", "Content", "Summary", ArticleCategory.Technology, new List<string> { "react", "frontend" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 3", "Content", "Summary", ArticleCategory.Technology, new List<string> { "dotnet", "backend" }));

        // Act: Requests filtered summaries (Anonymous access allowed).
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/articles/summaries?tags=dotnet");
        response.EnsureSuccessStatusCode();

        var pagedResult = await response.Content.ReadFromJsonAsync<PagedSummaryResponse>();

        // Assert: Verifies matching logic and total count.
        Assert.NotNull(pagedResult);
        Assert.Equal(2, pagedResult.Items.Count);
        Assert.Equal(2, pagedResult.TotalCount);
        Assert.All(pagedResult.Items, item => Assert.Contains("dotnet", item.Tags));
    }

    [Fact]
    public async Task GetSummaries_WithMultipleTagsFilter_ShouldReturnOnlyIntersection()
    {
        // Arrange: Authenticates and seeds data with overlapping tags.
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 1", "Content", "Summary", ArticleCategory.Technology, new List<string> { "dotnet", "cleancode", "csharp" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 2", "Content", "Summary", ArticleCategory.Technology, new List<string> { "dotnet", "csharp" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 3", "Content", "Summary", ArticleCategory.Technology, new List<string> { "cleancode" }));

        // Act: Performs multi-tag intersection filtering.
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/articles/summaries?tags=dotnet&tags=cleancode");
        response.EnsureSuccessStatusCode();

        var pagedResult = await response.Content.ReadFromJsonAsync<PagedSummaryResponse>();

        // Assert: Validates the AND (intersection) logic.
        Assert.NotNull(pagedResult);
        Assert.Single(pagedResult.Items);
        Assert.Equal(1, pagedResult.TotalCount);
        Assert.Equal("Title 1", pagedResult.Items[0].Title);
    }
}
