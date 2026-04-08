using AutoMapper;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Commands.DeleteArtigo;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles;

/**
 * Unit tests to ensure that the article cache is correctly invalidated during write operations.
 */
public class ArticleCacheInvalidationTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IMapper> _mapperMock;

    public ArticleCacheInvalidationTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task CreateArticle_ShouldInvalidateCache()
    {
        // Arrange
        var command = new CreateArticleCommand("Title", "Content", "Summary", ArticleCategory.Technology, new List<string>());
        var handler = new CreateArticleHandler(_repositoryMock.Object, _cacheMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert: Verify cache invalidation key was removed.
        _cacheMock.Verify(x => x.Remove("Articles_CacheVersion"), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Article>()), Times.Once);
    }

    [Fact]
    public async Task UpdateArticle_ShouldInvalidateCache_OnSuccess()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var command = new UpdateArticleCommand(articleId, "New Title", "New Content", "New Summary", ArticleCategory.Technology, new List<string>());
        var article = new Article("Old", "Old", "Old", new List<string>(), ArticleCategory.Technology);
        
        _repositoryMock.Setup(x => x.GetByIdAsync(articleId)).ReturnsAsync(article);
        _repositoryMock.Setup(x => x.UpdateAsync(article)).ReturnsAsync(true);
        
        var handler = new UpdateArticleCommandHandler(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(x => x.Remove("Articles_CacheVersion"), Times.Once);
    }

    [Fact]
    public async Task DeleteArticle_ShouldInvalidateCache_OnSuccess()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var command = new DeleteArticleCommand(articleId);
        
        _repositoryMock.Setup(x => x.DeleteAsync(articleId)).ReturnsAsync(true);
        
        var handler = new DeleteArticleCommandHandler(_repositoryMock.Object, _cacheMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(x => x.Remove("Articles_CacheVersion"), Times.Once);
    }
}
