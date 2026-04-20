using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// Comprehensive integration tests for article management and discovery.
/// </summary>
public class ArticlesIntegrationTests : BaseIntegrationTest
{
    // HC: Updated local record to match the real ArticleResponse from Application layer, including Category.
    public record ArticleResponse(Guid Id, string Title, string Content, string Summary, DateTime CreatedAt, List<string> Tags, ArticleCategory Category);
    public record PagedArticleResponse(List<ArticleResponse> Items, int CurrentPage, int TotalPages, int TotalCount);

    [Fact]
    public async Task PostArticle_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange: Authenticates and prepares valid article data.
        await AuthenticateAsync();
        var command = new CreateArticleCommand(
            "Integration Test Title",
            "Título de Teste de Integração",
            "This is a full content for integration testing.",
            "Conteúdo completo para teste de integração.",
            "Quick summary for testing.",
            "Resumo rápido para teste.",
            ArticleCategory.Technology,
            new List<string> { "integration", "test", "dotnet" }
        );

        // Act: Persists the new article.
        var response = await _client.PostAsJsonAsync("/api/articles", command);
        
        // Assert: Verifies resource creation and retrieval.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);

        var getResponse = await _client.GetAsync(location);
        getResponse.EnsureSuccessStatusCode();
        
        var persistedArticle = await getResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        Assert.NotNull(persistedArticle);
        Assert.Equal("Integration Test Title", persistedArticle.Title);
        Assert.Equal(ArticleCategory.Technology, persistedArticle.Category);
    }

    [Fact]
    public async Task ListArticles_WithPagination_ShouldReturnMetadataAndCorrectCount()
    {
        // Arrange: Seed database with articles. Requires authentication for setup.
        await AuthenticateAsync();
        for (int i = 1; i <= 3; i++)
        {
            var command = new CreateArticleCommand(
                $"Article {i}", $"Artigo {i}",
                $"Content {i}", $"Conteúdo {i}",
                $"Summary {i}", $"Resumo {i}",
                ArticleCategory.Technology, new List<string>());
            await _client.PostAsJsonAsync("/api/articles", command);
        }

        // Act: Requests paginated list (Anonymous access allowed for GET).
        _client.DefaultRequestHeaders.Authorization = null; 
        var response = await _client.GetAsync("/api/articles?pageNumber=1&pageSize=2");

        // Assert: Verifies pagination logic and anonymous visibility.
        response.EnsureSuccessStatusCode();
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedArticleResponse>();
        
        Assert.NotNull(pagedResult);
        Assert.Equal(2, pagedResult.Items.Count);
        Assert.True(pagedResult.TotalCount >= 3);
        Assert.Equal(1, pagedResult.CurrentPage);
    }
    
    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnArticle()
    {
        // Arrange: Seeds an article.
        await AuthenticateAsync();
        var command = new CreateArticleCommand(
            "Search by ID", "Busca por ID",
            "Content", "Conteúdo",
            "Summary", "Resumo",
            ArticleCategory.Technology, new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/articles", command);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: Retrieves article by ID (Anonymous access allowed).
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/articles/{id}");

        // Assert: Verifies retrieval and anonymous visibility.
        response.EnsureSuccessStatusCode();
        var article = await response.Content.ReadFromJsonAsync<ArticleResponse>();
        Assert.NotNull(article);
        Assert.Equal("Search by ID", article.Title);
    }
    
    [Fact]
    public async Task Update_WithValidData_ShouldModifyArticleInDatabase()
    {
        // Arrange: Seeds an article and prepares update data.
        await AuthenticateAsync();
        var createCommand = new CreateArticleCommand(
            "Original", "Original",
            "Content", "Conteúdo",
            "Summary", "Resumo",
            ArticleCategory.Technology, new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/articles", createCommand);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateCommand = new UpdateArticleCommand(
            id, 
            "Updated Title", "Título Atualizado",
            "New Content", "Novo Conteúdo",
            "New Summary", "Novo Resumo",
            ArticleCategory.Tutorial, new List<string> { "updated" });

        // Act: Updates the article.
        var response = await _client.PutAsJsonAsync($"/api/articles/{id}", updateCommand);

        // Assert: Verifies the update was successful.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/articles/{id}");
        var updatedArticle = await getResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        Assert.Equal("Updated Title", updatedArticle?.Title);
        Assert.Equal(ArticleCategory.Tutorial, updatedArticle?.Category);
    }

    [Fact]
    public async Task Delete_WithExistingId_ShouldRemoveArticleFromDatabase()
    {
        // Arrange: Seeds an article for deletion.
        await AuthenticateAsync();
        var command = new CreateArticleCommand(
            "For Deletion", "Para Deleção",
            "Content", "Conteúdo",
            "Summary", "Resumo",
            ArticleCategory.Technology, new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/articles", command);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: Deletes the article.
        var response = await _client.DeleteAsync($"/api/articles/{id}");

        // Assert: Verifies the deletion.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/articles/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}