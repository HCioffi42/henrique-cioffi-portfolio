using System.Net.Http.Json;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// Integration tests specifically for the article summary pagination logic.
/// </summary>
public class ArticleSummaryPaginationTests : BaseIntegrationTest
{
    public record ArticleSummaryResponse(Guid Id, string Title, string Summary, DateTime CreationDate, List<string> Tags);
    public record PagedSummaryResponse(List<ArticleSummaryResponse> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages);

    [Fact]
    public async Task GetSummaries_WithPagination_ShouldReturnPagedResult()
    {
        // Arrange: Authenticates for data setup.
        await AuthenticateAsync();

        // Seeds the database with multiple articles.
        for (int i = 1; i <= 5; i++)
        {
            var command = new CreateArticleCommand($"Summary Article {i}", "Full content", $"Summary {i}", new List<string> { "tag" });
            await _client.PostAsJsonAsync("/api/articles", command);
        }

        // Act: Requests paginated summaries (Anonymous access allowed).
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/articles/summaries?pageNumber=1&pageSize=3");

        // Assert: Validates pagination metadata and response projection.
        response.EnsureSuccessStatusCode();
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedSummaryResponse>();

        Assert.NotNull(pagedResult);
        Assert.Equal(3, pagedResult.Items.Count);
        Assert.True(pagedResult.TotalCount >= 5);
        Assert.Equal(1, pagedResult.PageNumber);
        Assert.Equal(3, pagedResult.PageSize);
        Assert.True(pagedResult.TotalPages >= 2);
        
        // Verify projection (no content field)
        var firstItemJson = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"content\"", firstItemJson.ToLower());
    }
}
