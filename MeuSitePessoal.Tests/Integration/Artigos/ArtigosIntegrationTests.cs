using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

public class ArtigosIntegrationTests : BaseIntegrationTest
{
    // Define a simple record to represent the response structure
    public record ArtigoResponse(Guid Id, string Titulo, string Conteudo, string Resumo, List<string> Tags);
    
    // Updated record to match the PagedList JSON structure
    public record PagedArtigoResponse(List<ArtigoResponse> Items, int CurrentPage, int TotalPages, int TotalCount);

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
    }

    [Fact]
    public async Task ListarArtigos_ComPaginacao_DeveRetornarMetadadosECountCorreto()
    {
        // Arrange
        await EnsureAuthenticatedAsync();
        
        // Seed database with 3 articles
        for (int i = 1; i <= 3; i++)
        {
            var command = new CreateArtigoCommand($"Artigo {i}", $"Cont {i}", $"Res {i}", new List<string>());
            await _client.PostAsJsonAsync("/api/artigos", command);
        }

        // Act: Request page 1 with size 2
        var response = await _client.GetAsync("/api/artigos?pageNumber=1&pageSize=2");

        // Assert
        response.EnsureSuccessStatusCode();
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedArtigoResponse>();
        
        Assert.NotNull(pagedResult);
        Assert.Equal(2, pagedResult.Items.Count); // Should only return 2 items due to pageSize
        Assert.True(pagedResult.TotalCount >= 3); // Total database count
        Assert.Equal(1, pagedResult.CurrentPage);
        Assert.Contains(pagedResult.Items, a => a.Titulo.StartsWith("Artigo"));
    }
    
    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornarArtigo()
    {
        // Arrange: Garante autenticação e cria um artigo para ser buscado
        await EnsureAuthenticatedAsync();
        var command = new CreateArtigoCommand("Busca por ID", "Conteudo", "Resumo", new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/artigos", command);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: Tenta obter o artigo recém-criado pelo seu identificador único
        var response = await _client.GetAsync($"/api/artigos/{id}");

        // Assert: Verifica se o retorno é sucesso e se os dados coincidem
        response.EnsureSuccessStatusCode();
        var artigo = await response.Content.ReadFromJsonAsync<ArtigoResponse>();
        Assert.NotNull(artigo);
        Assert.Equal("Busca por ID", artigo.Titulo);
    }
    
    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveAlterarArtigoNoBanco()
    {
        // Arrange: Cria um artigo e prepara os novos dados de atualização
        await EnsureAuthenticatedAsync();
        var createCommand = new CreateArtigoCommand("Original", "Conteudo", "Resumo", new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/artigos", createCommand);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateCommand = new UpdateArtigoCommand(id, "Titulo Atualizado", "Novo Conteudo", "Novo Resumo", new List<string> { "atualizado" });

        // Act: Envia a requisição PUT para o endpoint de atualização
        var response = await _client.PutAsJsonAsync($"/api/artigos/{id}", updateCommand);

        // Assert: Confirma o status de sem conteúdo (NoContent) e valida a alteração via GET
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/artigos/{id}");
        var artigoAtualizado = await getResponse.Content.ReadFromJsonAsync<ArtigoResponse>();
        Assert.Equal("Titulo Atualizado", artigoAtualizado?.Titulo);
    }

    [Fact]
    public async Task Excluir_ComIdExistente_DeveRemoverArtigoDoBanco()
    {
        // Arrange: Cria um artigo que será removido no passo seguinte
        await EnsureAuthenticatedAsync();
        var command = new CreateArtigoCommand("Para Excluir", "Conteudo", "Resumo", new List<string>());
        var createResponse = await _client.PostAsJsonAsync("/api/artigos", command);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act: Executa a operação de exclusão
        var deleteResponse = await _client.DeleteAsync($"/api/artigos/{id}");

        // Assert: Verifica se a exclusão foi bem-sucedida e se o artigo não existe mais
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/artigos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}