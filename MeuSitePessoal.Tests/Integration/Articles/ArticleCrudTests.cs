using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Articles;

public class ArticleCrudTests : BaseIntegrationTest
{
    // Record defines the structure of the article response for test assertions.
    private record ArticleResponse(Guid Id, string Title, string Content, string Summary, List<string> Tags);

    [Fact]
    public async Task Update_WithValidData_ShouldReturn204NoContent()
    {
        // Arrange: Authenticates the client and seeds an article.
        await AuthenticateAsync(); 
        var id = await SeedArticleAsync();
        var updateCommand = new UpdateArticleCommand(
            id,
            "Updated Title",
            "Updated content for this article.",
            "Updated summary.",
            ArticleCategory.Technology,
            new List<string> { "updated", "test" }
        );

        // Act: Performs the update request.
        var response = await _client.PutAsJsonAsync($"/api/Articles/{id}", updateCommand);

        // Assert: Verifies success and data integrity.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/Articles/{id}");
        var updatedArticle = await getResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        
        Assert.NotNull(updatedArticle);
        Assert.Equal("Updated Title", updatedArticle.Title);
        Assert.Contains("updated", updatedArticle.Tags);
    }

    [Fact]
    public async Task Update_WhenIdDoesNotExist_ShouldReturn404NotFound()
    {
        // Arrange: Authenticates and generates a non-existent ID.
        await AuthenticateAsync();
        var nonExistentId = Guid.NewGuid();
        var updateCommand = new UpdateArticleCommand(nonExistentId, "Title", "Content", "Summary", ArticleCategory.Technology, new List<string>());

        // Act: Attempts to update a missing resource.
        var response = await _client.PutAsJsonAsync($"/api/Articles/{nonExistentId}", updateCommand);

        // Assert: Verifies the Not Found response.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange: Authenticates and seeds data.
        await AuthenticateAsync();
        var id = await SeedArticleAsync();

        // Act: Deletes the article.
        var response = await _client.DeleteAsync($"/api/Articles/{id}");

        // Assert: Verifies removal and subsequent 404.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _client.GetAsync($"/api/Articles/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenIdDoesNotExist_ShouldReturn404NotFound()
    {
        // Arrange: Authenticates and generates a non-existent ID.
        await AuthenticateAsync();
        var nonExistentId = Guid.NewGuid();

        // Act: Attempts to delete a missing resource.
        var response = await _client.DeleteAsync($"/api/Articles/{nonExistentId}");

        // Assert: Verifies the Not Found response.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenAnonymous_ShouldReturn401Unauthorized()
    {
        // Arrange: No authentication call is made.
        var command = new CreateArticleCommand("Title", "Content", "Summary", ArticleCategory.Technology, new List<string>());

        // Act: Attempts to create without credentials.
        var response = await _client.PostAsJsonAsync("/api/Articles", command);

        // Assert: Verifies the authorization gate is working.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Seeds an article into the database to provide data for Update and Delete tests.
    /// </summary>
    private async Task<Guid> SeedArticleAsync()
    {
        var command = new CreateArticleCommand(
            "Seeded Title",
            "Original content for seeding.",
            "Original summary for seeding.",
            ArticleCategory.Technology,
            new List<string> { "seed" }
        );

        var response = await _client.PostAsJsonAsync("/api/Articles", command);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}