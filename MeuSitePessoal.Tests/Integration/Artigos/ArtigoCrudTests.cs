using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Artigos;

public class ArtigoCrudTests : BaseIntegrationTest
{
    private record ArtigoResponse(Guid Id, string Titulo, string Conteudo, string Resumo, List<string> Tags);

    [Fact]
    public async Task Update_WithValidData_ShouldReturn204NoContent()
    {
        // Arrange: Seed an article first.
        var id = await SeedArtigoAsync();
        var updateCommand = new UpdateArtigoCommand(
            id,
            "Updated Title",
            "Updated content for this article.",
            "Updated summary.",
            new List<string> { "updated", "test" }
        );

        // Act: Perform the update.
        var response = await _client.PutAsJsonAsync($"/api/Artigos/{id}", updateCommand);

        // Assert: Verify the response and that the data was actually updated.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/Artigos/{id}");
        var updatedArtigo = await getResponse.Content.ReadFromJsonAsync<ArtigoResponse>();
        
        Assert.NotNull(updatedArtigo);
        Assert.Equal("Updated Title", updatedArtigo.Titulo);
        Assert.Equal("Updated summary.", updatedArtigo.Resumo);
        Assert.Contains("updated", updatedArtigo.Tags);
    }

    [Fact]
    public async Task Update_WhenIdDoesNotExist_ShouldReturn404NotFound()
    {
        // Arrange: Use a non-existent Guid.
        var nonExistentId = Guid.NewGuid();
        var updateCommand = new UpdateArtigoCommand(
            nonExistentId,
            "Title",
            "Content",
            "Summary",
            new List<string>()
        );

        // Act: Try to update.
        var response = await _client.PutAsJsonAsync($"/api/Artigos/{nonExistentId}", updateCommand);

        // Assert: Verify 404 and the problem details title.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Resource Not Found", problem.Title);
    }

    [Fact]
    public async Task Update_WithValidationError_ShouldReturn400BadRequest()
    {
        // Arrange: Seed an article and prepare an invalid command (Empty Title).
        var id = await SeedArtigoAsync();
        var invalidCommand = new UpdateArtigoCommand(
            id,
            "", // Invalid: Empty Title
            "Valid content",
            "Valid summary",
            new List<string>()
        );

        // Act: Try to update.
        var response = await _client.PutAsJsonAsync($"/api/Artigos/{id}", invalidCommand);

        // Assert: Verify 400 and "Validation Error" title.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Validation Error", problem.Title);
        Assert.True(problem.Errors.ContainsKey("Titulo"));
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange: Seed an article.
        var id = await SeedArtigoAsync();

        // Act: Delete the article.
        var response = await _client.DeleteAsync($"/api/Artigos/{id}");

        // Assert: Verify success and that the article is no longer accessible.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/Artigos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenIdDoesNotExist_ShouldReturn404NotFound()
    {
        // Arrange: Use a non-existent Guid.
        var nonExistentId = Guid.NewGuid();

        // Act: Try to delete.
        var response = await _client.DeleteAsync($"/api/Artigos/{nonExistentId}");

        // Assert: Verify 404.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Helper method to seed an article into the database before Update/Delete actions.
    /// Uses the POST endpoint to ensure the full flow is valid.
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

        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return id;
    }
}
