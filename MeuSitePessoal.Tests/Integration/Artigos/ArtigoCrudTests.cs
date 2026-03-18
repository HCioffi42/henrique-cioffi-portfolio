using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Artigos;

public class ArtigoCrudTests : BaseIntegrationTest
{
    // Record defines the structure of the article response for test assertions.
    private record ArtigoResponse(Guid Id, string Titulo, string Conteudo, string Resumo, List<string> Tags);

    [Fact]
    public async Task Update_WithValidData_ShouldReturn204NoContent()
    {
        // Arrange: Authenticates the client and seeds an article.
        await AuthenticateAsync(); 
        var id = await SeedArtigoAsync();
        var updateCommand = new UpdateArtigoCommand(
            id,
            "Updated Title",
            "Updated content for this article.",
            "Updated summary.",
            new List<string> { "updated", "test" }
        );

        // Act: Performs the update request.
        var response = await _client.PutAsJsonAsync($"/api/Artigos/{id}", updateCommand);

        // Assert: Verifies success and data integrity.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/Artigos/{id}");
        var updatedArtigo = await getResponse.Content.ReadFromJsonAsync<ArtigoResponse>();
        
        Assert.NotNull(updatedArtigo);
        Assert.Equal("Updated Title", updatedArtigo.Titulo);
        Assert.Contains("updated", updatedArtigo.Tags);
    }

    [Fact]
    public async Task Update_WhenIdDoesNotExist_ShouldReturn404NotFound()
    {
        // Arrange: Authenticates and generates a non-existent ID.
        await AuthenticateAsync();
        var nonExistentId = Guid.NewGuid();
        var updateCommand = new UpdateArtigoCommand(nonExistentId, "Title", "Content", "Summary", new List<string>());

        // Act: Attempts to update a missing resource.
        var response = await _client.PutAsJsonAsync($"/api/Artigos/{nonExistentId}", updateCommand);

        // Assert: Verifies the Not Found response.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange: Authenticates and seeds data.
        await AuthenticateAsync();
        var id = await SeedArtigoAsync();

        // Act: Deletes the article.
        var response = await _client.DeleteAsync($"/api/Artigos/{id}");

        // Assert: Verifies removal and subsequent 404.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _client.GetAsync($"/api/Artigos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WhenAnonymous_ShouldReturn401Unauthorized()
    {
        // Arrange: No authentication call is made.
        var command = new CreateArtigoCommand("Title", "Content", "Summary", new List<string>());

        // Act: Attempts to create without credentials.
        var response = await _client.PostAsJsonAsync("/api/Artigos", command);

        // Assert: Verifies the authorization gate is working.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Seeds an article into the database to provide data for Update and Delete tests.
    /// </summary>
    private async Task<Guid> SeedArtigoAsync()
    {
        var command = new CreateArtigoCommand(
            "Seeded Title",
            "Original content for seeding.",
            "Original summary for seeding.",
            new List<string> { "seed" }
        );

        var response = await _client.PostAsJsonAsync("/api/Artigos", command);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}