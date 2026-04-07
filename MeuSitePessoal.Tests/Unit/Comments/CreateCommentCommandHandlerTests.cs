using MeuSitePessoal.Application.Comments.Commands.CreateComment;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Comments;

public class CreateCommentCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;

    public CreateCommentCommandHandlerTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            new Mock<IUserStore<IdentityUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

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

        var userId = "user-123";
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new IdentityUser { UserName = "Tester" });

        var handler = new CreateCommentCommandHandler(context, _currentUserServiceMock.Object, _userManagerMock.Object);
        var command = new CreateCommentCommand(article.Id, "Valid Comment");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == result.Value);
        Assert.NotNull(comment);
        Assert.Equal("Valid Comment", comment.Content);
        Assert.Equal(article.Id, comment.ArticleId);
        Assert.Equal(userId, comment.UserId);
        Assert.Equal("Tester", comment.AuthorName);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_NotAuthenticated()
    {
        // Arrange
        using var context = GetMemoryContext();
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new CreateCommentCommandHandler(context, _currentUserServiceMock.Object, _userManagerMock.Object);
        var command = new CreateCommentCommand(Guid.NewGuid(), "Comment");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User must be authenticated to comment.", result.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        // Arrange
        using var context = GetMemoryContext();
        var userId = "user-123";
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new IdentityUser { UserName = "Tester" });

        var handler = new CreateCommentCommandHandler(context, _currentUserServiceMock.Object, _userManagerMock.Object);
        var command = new CreateCommentCommand(Guid.NewGuid(), "Comment");

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

        var userId = "user-123";
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new IdentityUser { UserName = "Tester" });

        var handler = new CreateCommentCommandHandler(context, _currentUserServiceMock.Object, _userManagerMock.Object);
        var command = new CreateCommentCommand(article.Id, "Reply", parentComment.Id);

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

        var userId = "user-123";
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new IdentityUser { UserName = "Tester" });

        var handler = new CreateCommentCommandHandler(context, _currentUserServiceMock.Object, _userManagerMock.Object);
        var command = new CreateCommentCommand(article2.Id, "Reply", parentComment.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Type);
    }
}
