using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Artigos;

public class ArtigoExceptionTests : BaseIntegrationTest
{
    [Fact]
    public async Task Post_WhenTitleIsEmpty_ShouldReturn400BadRequest()
    {
        // Arrange
        var invalidCommand = new
        {
            Titulo = "", 
            Conteudo = "Valid content",
            Resumo = "Valid summary",
            Tags = new List<string> { "dotnet" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Artigos", invalidCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        
        Assert.NotNull(problem);
        Assert.Equal("Validation Error", problem.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.True(problem.Errors.ContainsKey("Titulo"));
        Assert.Contains("Title is required.", problem.Errors["Titulo"]);
    }

    [Fact]
    public async Task Post_WhenContentIsEmpty_ShouldReturn400BadRequest()
    {
        // Arrange
        var command = new
        {
            Titulo = "Valid Title",
            Conteudo = "", 
            Resumo = "Valid summary",
            Tags = new List<string> { "test" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Artigos", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Validation Error", problem.Title);
        Assert.True(problem.Errors.ContainsKey("Conteudo"));
        Assert.Contains("Content is required.", problem.Errors["Conteudo"]);
    }

    [Fact]
    public async Task Post_WhenSummaryIsEmpty_ShouldReturn400BadRequest()
    {
        // Arrange
        var command = new
        {
            Titulo = "Valid Title",
            Conteudo = "Valid content", 
            Resumo = "",
            Tags = new List<string> { "test" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Artigos", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("Resumo"));
        Assert.Contains("Summary is required.", problem.Errors["Resumo"]);
    }

    [Fact]
    public async Task Post_WhenFieldsExceedMaxLength_ShouldReturn400BadRequest()
    {
        // Arrange
        var command = new
        {
            Titulo = new string('a', 101),
            Conteudo = "Valid content", 
            Resumo = new string('b', 501),
            Tags = new List<string> { "test" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Artigos", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("Titulo"));
        Assert.Contains("Title must not exceed 100 characters.", problem.Errors["Titulo"]);
        Assert.True(problem.Errors.ContainsKey("Resumo"));
        Assert.Contains("Summary must not exceed 500 characters.", problem.Errors["Resumo"]);
    }
}
