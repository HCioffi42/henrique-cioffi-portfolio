using MeuSitePessoal.Application.Comments.Queries.GetCommentsByArticleId;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Comments;

public class GetCommentsByArticleIdQueryHandlerTests
{
    private BlogDbContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BlogDbContext(options);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        // Arrange
        using var context = GetMemoryContext();
        var handler = new GetCommentsByArticleIdQueryHandler(context);
        var query = new GetCommentsByArticleIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Type);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_ArticleHasNoComments()
    {
        // Arrange
        using var context = GetMemoryContext();
        var article = new Article("Title", "Content", "Summary", new List<string>(), ArticleCategory.Technology);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        var handler = new GetCommentsByArticleIdQueryHandler(context);
        var query = new GetCommentsByArticleIdQuery(article.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Handle_Should_ReturnHierarchicalTree_When_CommentsExist()
    {
        // Arrange
        using var context = GetMemoryContext();
        var article = new Article("Title", "Content", "Summary", new List<string>(), ArticleCategory.Technology);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        var root1 = new Comment(article.Id, "Root 1", "Author");
        var root2 = new Comment(article.Id, "Root 2", "Author");
        context.Comments.AddRange(root1, root2);
        await context.SaveChangesAsync();

        var reply1 = new Comment(article.Id, "Reply 1 to Root 1", "Author") { ParentCommentId = root1.Id };
        var reply2 = new Comment(article.Id, "Reply 2 to Root 1", "Author") { ParentCommentId = root1.Id };
        context.Comments.AddRange(reply1, reply2);
        await context.SaveChangesAsync();

        var nestedReply = new Comment(article.Id, "Nested Reply to Reply 1", "Author") { ParentCommentId = reply1.Id };
        context.Add(nestedReply);
        await context.SaveChangesAsync();

        var handler = new GetCommentsByArticleIdQueryHandler(context);
        var query = new GetCommentsByArticleIdQuery(article.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var tree = result.Value;
        Assert.Equal(2, tree!.Count); // root1 and root2

        var root1Response = tree.First(c => c.Id == root1.Id);
        Assert.Equal(2, root1Response.Replies.Count); // reply1 and reply2

        var reply1Response = root1Response.Replies.First(c => c.Id == reply1.Id);
        Assert.Single(reply1Response.Replies);
        Assert.Equal(nestedReply.Id, reply1Response.Replies[0].Id);
    }
}
