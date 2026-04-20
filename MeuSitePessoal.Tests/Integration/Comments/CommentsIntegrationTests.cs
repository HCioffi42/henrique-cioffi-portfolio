using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Comments.Commands.CreateComment;
using MeuSitePessoal.Application.Comments.Queries.GetCommentsByArticleId;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Comments;

public class CommentsIntegrationTests : BaseIntegrationTest
{
    private async Task<Guid> CreateArticleAsync()
    {
        await AuthenticateAsync();
        var request = new
        {
            TitleEn = "Integration Test Article",
            TitlePt = "Artigo de Teste de Integração",
            ContentEn = "Content for integration test.",
            ContentPt = "Conteúdo para teste de integração.",
            SummaryEn = "Summary",
            SummaryPt = "Resumo",
            Tags = new List<string> { "Test" },
            Category = ArticleCategory.Technology
        };

        var response = await _client.PostAsJsonAsync("/api/articles", request);
        response.EnsureSuccessStatusCode();
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return id;
    }

    [Fact]
    public async Task CreateComment_Should_ReturnCreated_When_RequestIsValid()
    {
        // Arrange
        var articleId = await CreateArticleAsync();
        var command = new CreateCommentCommand(articleId, "Integration Test Comment");

        // Act
        var response = await _client.PostAsJsonAsync("/api/comments", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CommentIdResponse>();
        Assert.NotEqual(Guid.Empty, result?.Id);
    }

    private record CommentIdResponse(Guid Id);

    [Fact]
    public async Task CreateReply_Should_ReturnCreated_When_ParentExists()
    {
        // Arrange
        var articleId = await CreateArticleAsync();
        
        // 1. Create root comment
        var rootCommand = new CreateCommentCommand(articleId, "Root Comment");
        var rootResponse = await _client.PostAsJsonAsync("/api/comments", rootCommand);
        var rootResult = await rootResponse.Content.ReadFromJsonAsync<CommentIdResponse>();
        Guid rootId = rootResult!.Id;

        // 2. Create reply
        var replyCommand = new CreateCommentCommand(articleId, "Reply Comment", rootId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/comments", replyCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetComments_Should_ReturnTreeStructure()
    {
        // Arrange
        var articleId = await CreateArticleAsync();
        
        // 1. Create root comment
        var rootCommand = new CreateCommentCommand(articleId, "Root");
        var rootResponse = await _client.PostAsJsonAsync("/api/comments", rootCommand);
        var rootResult = await rootResponse.Content.ReadFromJsonAsync<CommentIdResponse>();
        Guid rootId = rootResult!.Id;

        // 2. Create reply to root
        var replyCommand = new CreateCommentCommand(articleId, "Reply 1", rootId);
        await _client.PostAsJsonAsync("/api/comments", replyCommand);

        // Act
        var response = await _client.GetAsync($"/api/articles/{articleId}/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var tree = await response.Content.ReadFromJsonAsync<List<CommentResponse>>();
        
        Assert.NotNull(tree);
        Assert.Single(tree);
        Assert.Equal("Root", tree[0].Content);
        Assert.Single(tree[0].Replies);
        Assert.Equal("Reply 1", tree[0].Replies[0].Content);
    }

    [Fact]
    public async Task GetComments_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/articles/{Guid.NewGuid()}/comments");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}