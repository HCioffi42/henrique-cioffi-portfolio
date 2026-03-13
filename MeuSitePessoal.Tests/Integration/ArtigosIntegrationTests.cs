using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Api;
using MeuSitePessoal.Application.Commands;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

public class ArtigosIntegrationTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public ArtigosIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Initializes the HttpClient to point to the in-memory test server
        _client = factory.CreateClient();
    }
    
    // Define a simple record to represent the response structure
    public record ArtigoResponse(Guid Id, string Titulo, string Conteudo, string Resumo, List<string> Tags);
    
    [Fact]
    public async Task PostArtigo_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange: Prepare a command with real data for integration testing
        var command = new CreateArtigoCommand(
            "Integration Test Title",
            "This is a full content for integration testing.",
            "Quick summary for testing.",
            new List<string> { "integration", "test", "dotnet" }
        );

        // Act: Send a real POST request to the API endpoint
        var response = await _client.PostAsJsonAsync("/api/artigos", command);

        // Assert: Verify if the API returned 201 Created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Optional: Follow the location header to verify persistence via GET
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);

        var getResponse = await _client.GetAsync(location);
        getResponse.EnsureSuccessStatusCode();
        
        // Verify if the returned data matches what was sent
        var persistedArtigo = await getResponse.Content.ReadFromJsonAsync<ArtigoResponse>();
        // Now you have full IntelliSense and type safety in Rider
        Assert.NotNull(persistedArtigo);
        Assert.Equal("Integration Test Title", persistedArtigo.Titulo);
        Assert.Equal("Quick summary for testing.", persistedArtigo.Resumo);
        Assert.Equal(3, persistedArtigo.Tags.Count);
        
        await _client.DeleteAsync($"/api/artigos/{persistedArtigo.Id}");
    }
}