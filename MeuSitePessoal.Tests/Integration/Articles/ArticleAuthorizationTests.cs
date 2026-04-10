using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Articles;

public class ArticleAuthorizationTests : BaseIntegrationTest
{
    private async Task AuthenticateAsReaderAsync()
    {
        // 1. Create a reader user
        var registerRequest = new
        {
            UserName = "reader",
            Email = "reader@test.com",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        await ConfirmUserEmailAsync(registerRequest.UserName);

        // 2. Login as reader
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "reader",
            Password = "Password123!"
        });
        var result = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Token);
    }

    private record AuthResult(string Token, string Username);

    [Fact]
    public async Task CreateArticle_Should_ReturnForbidden_When_UserIsReader()
    {
        // Arrange
        await AuthenticateAsReaderAsync();
        var request = new
        {
            Title = "Forbidden Article",
            Content = "Content",
            Summary = "Summary",
            Tags = new List<string>(),
            Category = ArticleCategory.Technology
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/articles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateArticle_Should_ReturnForbidden_When_UserIsReader()
    {
        // Arrange
        // 1. Admin creates article
        await AuthenticateAsync();
        var createRequest = new
        {
            Title = "Admin Article",
            Content = "Content",
            Summary = "Summary",
            Tags = new List<string>(),
            Category = ArticleCategory.Technology
        };
        var createResponse = await _client.PostAsJsonAsync("/api/articles", createRequest);
        var articleId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // 2. Reader attempts to update
        await AuthenticateAsReaderAsync();
        var updateRequest = new
        {
            Id = articleId,
            Title = "Updated by Reader",
            Content = "Updated Content",
            Summary = "Summary",
            Tags = new List<string>(),
            Category = ArticleCategory.Technology
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/articles/{articleId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteArticle_Should_ReturnForbidden_When_UserIsReader()
    {
        // Arrange
        // 1. Admin creates article
        await AuthenticateAsync();
        var createRequest = new
        {
            Title = "Admin Article",
            Content = "Content",
            Summary = "Summary",
            Tags = new List<string>(),
            Category = ArticleCategory.Technology
        };
        var createResponse = await _client.PostAsJsonAsync("/api/articles", createRequest);
        var articleId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // 2. Reader attempts to delete
        await AuthenticateAsReaderAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/articles/{articleId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
