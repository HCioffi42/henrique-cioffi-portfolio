using System.Net.Http.Json;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Artigos;

public class ArtigoSummaryTagFilterTests : BaseIntegrationTest
{
    public record AuthResponse(string Token);
    public record ArtigoSummaryResponse(Guid Id, string Titulo, string Resumo, DateTime DataCriacao, List<string> Tags);
    public record PagedSummaryResponse(List<ArtigoSummaryResponse> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages);

    [Fact]
    public async Task GetSummaries_WithTagFilter_ShouldReturnOnlyMatchingArticles()
    {
        // Arranges the authentication token for restricted endpoint access.
        var loginRequest = new { Username = "admin", Password = "admin123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);

        // Seeds specific data for tag filtering validation.
        await _client.PostAsJsonAsync("/api/artigos", new CreateArtigoCommand("Title 1", "Content", "Summary", new List<string> { "dotnet", "csharp" }));
        await _client.PostAsJsonAsync("/api/artigos", new CreateArtigoCommand("Title 2", "Content", "Summary", new List<string> { "react", "frontend" }));
        await _client.PostAsJsonAsync("/api/artigos", new CreateArtigoCommand("Title 3", "Content", "Summary", new List<string> { "dotnet", "backend" }));

        // Acts by requesting the summaries endpoint with the 'dotnet' tag filter.
        var response = await _client.GetAsync("/api/artigos/summaries?tag=dotnet");
        response.EnsureSuccessStatusCode();

        var pagedResult = await response.Content.ReadFromJsonAsync<PagedSummaryResponse>();

        // Asserts that only the items containing the specified tag are returned and the total count is accurate.
        Assert.NotNull(pagedResult);
        Assert.Equal(2, pagedResult.Items.Count);
        Assert.Equal(2, pagedResult.TotalCount);
        Assert.All(pagedResult.Items, item => Assert.Contains("dotnet", item.Tags));
    }
}