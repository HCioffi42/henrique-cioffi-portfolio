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
    public record ArticleResponse(Guid Id, string Title, string Content, string Summary, List<string> Tags, ArticleCategory Category);
    public record PagedArtigoResponse(List<ArticleResponse> Items, int CurrentPage, int TotalPages, int TotalCount);

    [Fact]
    public async Task PostArtigo_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange: Authenticates and prepares valid article data.
        await AuthenticateAsync();
        var command = new CreateArticleCommand(
            "Integration Test Title",
            "This is a full content for integration testing.",
            "Quick summary for testing.",
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
        
        var persistedArtigo = await getResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        Assert.NotNull(persistedArtigo);
        Assert.Equal("Integration Test Title", persistedArtigo.Title);
        Assert.Equal(ArticleCategory.Technology, persistedArtigo.Category);
    }

    [Fact]
    public async Task ListarArtigos_ComPaginacao_DeveRetornarMetadadosECountCorreto()
    {
        // Arrange: Seed database with articles. Requires authentication for setup.
        await AuthenticateAsync();
        for (int i = 1; i <= 3; i++)
        {
            var command = new CreateArticleCommand($"Article {i}", $"Cont {i}", $"Res {i}", ArticleCategory.Technology, new List<string>());
            await _client.PostAsJsonAsync("/api/articles", command);
        }

        // Act: Requests paginated list (Anonymous access allowed for GET).
        _client.DefaultRequestHeaders.Authorization = null; 
        var response = await _client.GetAsync("/api/articles?pageNumber=1&pageSize=2");

        // Assert: Verifies pagination logic and anonymous visibility.
        response.EnsureSuccessStatusCode();
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedArtigoResponse>();
        
        Assert.NotNull(pagedResult);
        Assert.Equal(2, pagedResult.Items.Count);
        Assert.True(pagedResult.TotalCount >= 3);
        Assert.Equal(1, pagedResult.CurrentPage);
    }
    
    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornarArtigo()
    {
        // Arrange: Seeds an article.
        await AuthenticateAsync();
        var command = new CreateArticleCommand("Busca por ID", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/articles", command);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: Retrieves article by ID (Anonymous access allowed).
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/articles/{id}");

        // Assert: Verifies retrieval and anonymous visibility.
        response.EnsureSuccessStatusCode();
        var artigo = await response.Content.ReadFromJsonAsync<ArticleResponse>();
        Assert.NotNull(artigo);
        Assert.Equal("Busca por ID", artigo.Title);
    }
    
    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveAlterarArtigoNoBanco()
    {
        // Arrange: Seeds an article and prepares update data.
        await AuthenticateAsync();
        var createCommand = new CreateArticleCommand("Original", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/articles", createCommand);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateCommand = new UpdateArticleCommand(id, "Title Atualizado", "Novo Content", "Novo Summary", ArticleCategory.Tutorial, new List<string> { "atualizado" });

        // Act: Updates the article.
        var response = await _client.PutAsJsonAsync($"/api/articles/{id}", updateCommand);

        // Assert: Verifies the update was successful.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/articles/{id}");
        var artigoAtualizado = await getResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        Assert.Equal("Title Atualizado", artigoAtualizado?.Title);
        Assert.Equal(ArticleCategory.Tutorial, artigoAtualizado?.Category);
    }

    [Fact]
    public async Task Excluir_ComIdExistente_DeveRemoverArtigoDoBanco()
    {
        // Arrange: Seeds an article for deletion.
        await AuthenticateAsync();
        var command = new CreateArticleCommand("Para Delete", "Content", "Summary", ArticleCategory.Technology, new List<string>());
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
