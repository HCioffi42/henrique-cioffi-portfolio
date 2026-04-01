using MeuSitePessoal.Application.Articles.Commands.DeleteArtigo;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

/// <summary>
/// Unit tests for the DeleteArticleCommandHandler, validating the deletion flow and repository interaction.
/// </summary>
public class DeleteArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly DeleteArticleCommandHandler _handler;

    public DeleteArticleHandlerTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _handler = new DeleteArticleCommandHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteArticleCommand(id);
        _repositoryMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteArticleCommand(id);
        _repositoryMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
