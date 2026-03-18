using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

/// <summary>
/// Comprehensive integration tests for article management and discovery.
/// </summary>
public class ArtigosIntegrationTests : BaseIntegrationTest
{
    public record ArtigoResponse(Guid Id, string Titulo, string Conteudo, string Resumo, List<string> Tags);
    public record PagedArtigoResponse(List<ArtigoResponse> Items, int CurrentPage, int TotalPages, int TotalCount);

    [Fact]
    public async Task PostArtigo_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange: Authenticates and prepares valid article data.
        await AuthenticateAsync();
        var command = new CreateArtigoCommand(
            "Integration Test Title",
            "This is a full content for integration testing.",
            "Quick summary for testing.",
            new List<string> { "integration", "test", "dotnet" }
        );

        // Act: Persists the new article.
        var response = await _client.PostAsJsonAsync("/api/artigos", command);
        
        // Assert: Verifies resource creation and retrieval.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);

        var getResponse = await _client.GetAsync(location);
        getResponse.EnsureSuccessStatusCode();
        
        var persistedArtigo = await getResponse.Content.ReadFromJsonAsync<ArtigoResponse>();
        Assert.NotNull(persistedArtigo);
        Assert.Equal("Integration Test Title", persistedArtigo.Titulo);
    }

    [Fact]
    public async Task ListarArtigos_ComPaginacao_DeveRetornarMetadadosECountCorreto()
    {
        // Arrange: Seed database with articles. Requires authentication for setup.
        await AuthenticateAsync();
        for (int i = 1; i <= 3; i++)
        {
            var command = new CreateArtigoCommand($"Artigo {i}", $"Cont {i}", $"Res {i}", new List<string>());
            await _client.PostAsJsonAsync("/api/artigos", command);
        }

        // Act: Requests paginated list (Anonymous access allowed for GET).
        _client.DefaultRequestHeaders.Authorization = null; 
        var response = await _client.GetAsync("/api/artigos?pageNumber=1&pageSize=2");

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
        var command = new CreateArtigoCommand("Busca por ID", "Conteudo", "Resumo", new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/artigos", command);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: Retrieves article by ID (Anonymous access allowed).
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/artigos/{id}");

        // Assert: Verifies retrieval and anonymous visibility.
        response.EnsureSuccessStatusCode();
        var artigo = await response.Content.ReadFromJsonAsync<ArtigoResponse>();
        Assert.NotNull(artigo);
        Assert.Equal("Busca por ID", artigo.Titulo);
    }
    
    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveAlterarArtigoNoBanco()
    {
        // Arrange: Seeds an article and prepares update data.
        await AuthenticateAsync();
        var createCommand = new CreateArtigoCommand("Original", "Conteudo", "Resumo", new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/artigos", createCommand);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateCommand = new UpdateArtigoCommand(id, "Titulo Atualizado", "Novo Conteudo", "Novo Resumo", new List<string> { "atualizado" });

        // Act: Updates the article.
        var response = await _client.PutAsJsonAsync($"/api/artigos/{id}", updateCommand);

        // Assert: Verifies the update was successful.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/artigos/{id}");
        var artigoAtualizado = await getResponse.Content.ReadFromJsonAsync<ArtigoResponse>();
        Assert.Equal("Titulo Atualizado", artigoAtualizado?.Titulo);
    }

    [Fact]
    public async Task Excluir_ComIdExistente_DeveRemoverArtigoDoBanco()
    {
        // Arrange: Seeds an article for deletion.
        await AuthenticateAsync();
        var command = new CreateArtigoCommand("Para Excluir", "Conteudo", "Resumo", new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/artigos", command);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: Deletes the article.
        var response = await _client.DeleteAsync($"/api/artigos/{id}");

        // Assert: Verifies the deletion.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/artigos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
