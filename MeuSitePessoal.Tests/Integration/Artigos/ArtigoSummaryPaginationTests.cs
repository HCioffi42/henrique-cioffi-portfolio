using System.Net.Http.Json;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

public class ArtigoSummaryPaginationTests : BaseIntegrationTest
{
    public record ArtigoSummaryResponse(Guid Id, string Titulo, string Resumo, DateTime DataCriacao, List<string> Tags);
    public record PagedSummaryResponse(List<ArtigoSummaryResponse> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages);
    public record AuthResponse(string Token);

    [Fact]
    public async Task GetSummaries_WithPagination_ShouldReturnPagedResult()
    {
        // Arranges the login request to obtain the JWT token for restricted endpoint access.
        var loginRequest = new { Username = "admin", Password = "admin123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Sets the authorization header using the strongly typed token property.
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);
        // Seed some data
        for (int i = 1; i <= 5; i++)
        {
            var command = new CreateArtigoCommand($"Summary Artigo {i}", "Full content", $"Summary {i}", new List<string> { "tag" });
            await _client.PostAsJsonAsync("/api/artigos", command);
        }

        // Act
        var response = await _client.GetAsync("/api/artigos/summaries?pageNumber=1&pageSize=3");

        // Assert
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
        Assert.DoesNotContain("\"conteudo\"", firstItemJson.ToLower());
    }
}
