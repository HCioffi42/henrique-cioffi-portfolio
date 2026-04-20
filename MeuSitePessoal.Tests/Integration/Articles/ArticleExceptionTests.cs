using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// Integration tests for validating article creation constraints and exception handling.
/// </summary>
public class ArticleExceptionTests : BaseIntegrationTest
{
    [Fact]
    public async Task Post_WhenTitleIsEmpty_ShouldReturn400BadRequest()
    {
        // Arrange: Authenticates the client using the centralized method.
        await AuthenticateAsync();
        var invalidCommand = new
        {
            TitleEn = "", 
            TitlePt = "Título",
            ContentEn = "Valid content",
            ContentPt = "Conteúdo",
            SummaryEn = "Valid summary",
            SummaryPt = "Resumo",
            Category = 1,
            Tags = new List<string> { "dotnet" }
        };

        // Act: Sends an invalid request to the articles endpoint.
        var response = await _client.PostAsJsonAsync("/api/Articles", invalidCommand);

        // Assert: Verifies that the server returns a Bad Request response with validation details.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        
        Assert.NotNull(problem);
        Assert.Equal("Validation Error", problem.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.True(problem.Errors.ContainsKey("TitleEn"));
        Assert.Contains("English Title is required.", problem.Errors["TitleEn"]);
    }

    [Fact]
    public async Task Post_WhenContentIsEmpty_ShouldReturn400BadRequest()
    {
        // Arrange: Authenticates and prepares a command with missing content.
        await AuthenticateAsync();
        var command = new
        {
            TitleEn = "Valid Title",
            TitlePt = "Título",
            ContentEn = "", 
            ContentPt = "Conteúdo",
            SummaryEn = "Valid summary",
            SummaryPt = "Resumo",
            Category = 1,
            Tags = new List<string> { "test" }
        };

        // Act: Executes the post request.
        var response = await _client.PostAsJsonAsync("/api/Articles", command);

        // Assert: Validates the expected validation failure.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Validation Error", problem.Title);
        Assert.True(problem.Errors.ContainsKey("ContentEn"));
        Assert.Contains("English Content is required.", problem.Errors["ContentEn"]);
    }

    [Fact]
    public async Task Post_WhenSummaryIsEmpty_ShouldReturn400BadRequest()
    {
        // Arrange: Authenticates and prepares a command with an empty summary.
        await AuthenticateAsync();
        var command = new
        {
            TitleEn = "Valid Title",
            TitlePt = "Título",
            ContentEn = "Valid content", 
            ContentPt = "Conteúdo",
            SummaryEn = "",
            SummaryPt = "Resumo",
            Category = 1,
            Tags = new List<string> { "test" }
        };

        // Act: Executes the post request.
        var response = await _client.PostAsJsonAsync("/api/Articles", command);

        // Assert: Verifies that the summary requirement is enforced.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("SummaryEn"));
        Assert.Contains("English Summary is required.", problem.Errors["SummaryEn"]);
    }

    [Fact]
    public async Task Post_WhenFieldsExceedMaxLength_ShouldReturn400BadRequest()
    {
        // Arrange: Authenticates and prepares a command with oversized fields.
        await AuthenticateAsync();
        var command = new
        {
            TitleEn = new string('a', 101),
            TitlePt = "Título",
            ContentEn = "Valid content", 
            ContentPt = "Conteúdo",
            SummaryEn = new string('b', 501),
            SummaryPt = "Resumo",
            Category = 1,
            Tags = new List<string> { "test" }
        };

        // Act: Executes the request against the validator limits.
        var response = await _client.PostAsJsonAsync("/api/Articles", command);

        // Assert: Verifies that length constraints are triggered.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("TitleEn"));
        Assert.Contains("English Title must not exceed 100 characters.", problem.Errors["TitleEn"]);
        Assert.True(problem.Errors.ContainsKey("SummaryEn"));
        Assert.Contains("English Summary must not exceed 500 characters.", problem.Errors["SummaryEn"]);
    }
}