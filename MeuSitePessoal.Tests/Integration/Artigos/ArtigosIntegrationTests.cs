using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

public class ArtigosIntegrationTests : BaseIntegrationTest
{
    // Define a simple record to represent the response structure
    public record ArtigoResponse(Guid Id, string Titulo, string Conteudo, string Resumo, List<string> Tags);
    private string? _token;

    private async Task EnsureAuthenticatedAsync()
    {
        if (_token != null) return;

        var loginRequest = new { Username = "admin", Password = "admin123" };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        _token = authResponse!.Token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    private record AuthTokenResponse(string Token);
    
    [Fact]
    public async Task PostArtigo_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange
        await EnsureAuthenticatedAsync();
        var command = new CreateArtigoCommand(
            "Integration Test Title",
            "This is a full content for integration testing.",
            "Quick summary for testing.",
            new List<string> { "integration", "test", "dotnet" }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/artigos", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);

        var getResponse = await _client.GetAsync(location);
        getResponse.EnsureSuccessStatusCode();
        
        var persistedArtigo = await getResponse.Content.ReadFromJsonAsync<ArtigoResponse>();
        Assert.NotNull(persistedArtigo);
        Assert.Equal("Integration Test Title", persistedArtigo.Titulo);
        Assert.Equal("Quick summary for testing.", persistedArtigo.Resumo);
        Assert.Equal(3, persistedArtigo.Tags.Count);
    }

    [Fact]
    public async Task ListarArtigos_DeveRetornarTodosOsArtigosCadastrados()
    {
        // Arrange
        await EnsureAuthenticatedAsync();
        var command1 = new CreateArtigoCommand("Artigo 1", "Conteudo 1", "Resumo 1", new List<string>());
        var command2 = new CreateArtigoCommand("Artigo 2", "Conteudo 2", "Resumo 2", new List<string>());

        await _client.PostAsJsonAsync("/api/artigos", command1);
        await _client.PostAsJsonAsync("/api/artigos", command2);

        // Act
        var response = await _client.GetAsync("/api/artigos");

        // Assert
        response.EnsureSuccessStatusCode();
        var artigos = await response.Content.ReadFromJsonAsync<List<ArtigoResponse>>();
        
        Assert.NotNull(artigos);
        Assert.True(artigos.Count >= 2);
        Assert.Contains(artigos, a => a.Titulo == "Artigo 1");
        Assert.Contains(artigos, a => a.Titulo == "Artigo 2");
    }
}
