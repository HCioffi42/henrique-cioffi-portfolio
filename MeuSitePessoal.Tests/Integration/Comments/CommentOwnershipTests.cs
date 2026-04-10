using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Comments;

public class CommentOwnershipTests : BaseIntegrationTest
{
    private async Task<string> AuthenticateAsUserAsync(string username)
    {
        var registerRequest = new
        {
            UserName = username,
            Email = $"{username}@test.com",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        
        await ConfirmUserEmailAsync(username);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = username,
            Password = "Password123!"
        });
        var result = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Token);
        return result!.Token;
    }

    private record AuthResult(string Token, string Username);
    private record CommentIdResponse(Guid Id);

    private async Task<Guid> CreateArticleAsync()
    {
        await AuthenticateAsync();
        var request = new
        {
            Title = "Article for Comments",
            Content = "Content",
            Summary = "Summary",
            Tags = new List<string>(),
            Category = ArticleCategory.Technology
        };
        var response = await _client.PostAsJsonAsync("/api/articles", request);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    [Fact]
    public async Task Reader_Should_Only_Be_Able_To_Update_Own_Comment()
    {
        // Arrange
        var articleId = await CreateArticleAsync();

        // 1. User1 creates a comment
        await AuthenticateAsUserAsync("user1");
        var createResponse = await _client.PostAsJsonAsync("/api/comments", new { ArticleId = articleId, Content = "User1 Comment" });
        var commentId = (await createResponse.Content.ReadFromJsonAsync<CommentIdResponse>())!.Id;

        // 2. User2 attempts to update User1's comment
        await AuthenticateAsUserAsync("user2");
        var updateResponse = await _client.PutAsJsonAsync($"/api/comments/{commentId}", new { Id = commentId, Content = "Hacked" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);

        // 3. User1 updates own comment
        await AuthenticateAsUserAsync("user1");
        var ownUpdateResponse = await _client.PutAsJsonAsync($"/api/comments/{commentId}", new { Id = commentId, Content = "Updated by Owner" });
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, ownUpdateResponse.StatusCode);
    }

    [Fact]
    public async Task Reader_Should_Only_Be_Able_To_Delete_Own_Comment()
    {
        // Arrange
        var articleId = await CreateArticleAsync();

        // 1. User1 creates a comment
        await AuthenticateAsUserAsync("user1");
        var createResponse = await _client.PostAsJsonAsync("/api/comments", new { ArticleId = articleId, Content = "User1 Comment" });
        var commentId = (await createResponse.Content.ReadFromJsonAsync<CommentIdResponse>())!.Id;

        // 2. User2 attempts to delete User1's comment
        await AuthenticateAsUserAsync("user2");
        var deleteResponse = await _client.DeleteAsync($"/api/comments/{commentId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // 3. User1 deletes own comment
        await AuthenticateAsUserAsync("user1");
        var ownDeleteResponse = await _client.DeleteAsync($"/api/comments/{commentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, ownDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_Should_Be_Able_To_Update_And_Delete_Any_Comment()
    {
        // Arrange
        var articleId = await CreateArticleAsync();

        // 1. Reader creates a comment
        await AuthenticateAsUserAsync("reader");
        var createResponse = await _client.PostAsJsonAsync("/api/comments", new { ArticleId = articleId, Content = "Reader Comment" });
        var commentId = (await createResponse.Content.ReadFromJsonAsync<CommentIdResponse>())!.Id;

        // 2. Admin attempts to update and delete
        await AuthenticateAsync(); // Switch to seeded Admin
        
        var updateResponse = await _client.PutAsJsonAsync($"/api/comments/{commentId}", new { Id = commentId, Content = "Updated by Admin" });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/comments/{commentId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
