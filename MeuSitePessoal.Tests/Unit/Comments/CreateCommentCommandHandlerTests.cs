using MeuSitePessoal.Application.Comments.Commands.CreateComment;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Comments;

public class CreateCommentCommandHandlerTests
{
    private BlogDbContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BlogDbContext(options);
    }

    [Fact]
    public async Task Handle_Should_CreateComment_When_RequestIsValid()
    {
        // Arrange
        using var context = GetMemoryContext();
        var article = new Article("Title", "Content", "Summary", new List<string>(), ArticleCategory.Technology);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(context);
        var command = new CreateCommentCommand(article.Id, "Valid Comment", "Author");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == result.Value);
        Assert.NotNull(comment);
        Assert.Equal("Valid Comment", comment.Content);
        Assert.Equal(article.Id, comment.ArticleId);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        // Arrange
        using var context = GetMemoryContext();
        var handler = new CreateCommentCommandHandler(context);
        var command = new CreateCommentCommand(Guid.NewGuid(), "Comment", "Author");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Type);
    }

    [Fact]
    public async Task Handle_Should_CreateReply_When_ParentBelongsToSameArticle()
    {
        // Arrange
        using var context = GetMemoryContext();
        var article = new Article("Title", "Content", "Summary", new List<string>(), ArticleCategory.Technology);
        context.Articles.Add(article);
        
        var parentComment = new Comment(article.Id, "Parent", "Author");
        context.Comments.Add(parentComment);
        await context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(context);
        var command = new CreateCommentCommand(article.Id, "Reply", "Author", parentComment.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var reply = await context.Comments.FirstOrDefaultAsync(c => c.Id == result.Value);
        Assert.Equal(parentComment.Id, reply?.ParentCommentId);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_ParentBelongsToDifferentArticle()
    {
        // Arrange
        using var context = GetMemoryContext();
        var article1 = new Article("Title 1", "Content", "Summary", new List<string>(), ArticleCategory.Technology);
        var article2 = new Article("Title 2", "Content", "Summary", new List<string>(), ArticleCategory.Technology);
        context.Articles.AddRange(article1, article2);
        
        var parentComment = new Comment(article1.Id, "Parent", "Author");
        context.Comments.Add(parentComment);
        await context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(context);
        var command = new CreateCommentCommand(article2.Id, "Reply", "Author", parentComment.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Type);
    }
}
