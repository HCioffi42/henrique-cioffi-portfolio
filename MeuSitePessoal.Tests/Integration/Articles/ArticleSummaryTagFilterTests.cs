using System.Net.Http.Json;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// Integration tests for validating multi-tag filtering logic on article summaries.
/// </summary>
public class ArticleSummaryTagFilterTests : BaseIntegrationTest
{
    public record ArticleSummaryResponse(Guid Id, string Title, string Summary, DateTime CreationDate, List<string> Tags);
    public record PagedSummaryResponse(List<ArticleSummaryResponse> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages);

    [Fact]
    public async Task GetSummaries_WithSingleTagFilter_ShouldReturnMatchingArticles()
    {
        // Arrange: Authenticates for setup and seeds data.
        await AuthenticateAsync();
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 1", "Content", "Summary", new List<string> { "dotnet", "csharp" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 2", "Content", "Summary", new List<string> { "react", "frontend" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 3", "Content", "Summary", new List<string> { "dotnet", "backend" }));

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
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 1", "Content", "Summary", new List<string> { "dotnet", "cleancode", "csharp" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 2", "Content", "Summary", new List<string> { "dotnet", "csharp" }));
        await _client.PostAsJsonAsync("/api/articles", new CreateArticleCommand("Title 3", "Content", "Summary", new List<string> { "cleancode" }));

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
