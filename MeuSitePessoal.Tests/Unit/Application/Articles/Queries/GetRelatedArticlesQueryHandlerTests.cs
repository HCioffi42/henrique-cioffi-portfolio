using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Articles.Queries.GetRelatedArticles;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Queries;

/// <summary>
/// Unit tests for GetRelatedArticlesQueryHandler using an In-Memory database.
/// </summary>
public class GetRelatedArticlesQueryHandlerTests
{
    private readonly DbContextOptions<BlogDbContext> _options;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILanguageProvider> _languageProviderMock;

    public GetRelatedArticlesQueryHandlerTests()
    {
        _options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _cacheMock = new Mock<IMemoryCache>();
        
        _languageProviderMock = new Mock<ILanguageProvider>();
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("en");
    }

    private GetRelatedArticlesQueryHandler CreateHandler(BlogDbContext context)
    {
        return new GetRelatedArticlesQueryHandler(context, _cacheMock.Object, _languageProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithSharedTags_ShouldReturnRelatedArticles()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Base", "Base", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "dotnet", "web" }, ArticleCategory.Technology) { Id = baseId });
            context.Articles.Add(new Article("Related", "Relacionado", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "dotnet" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("Unrelated", "Não Relacionado", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "java" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        // Setup cache miss
        object? cacheValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        using (var context = new BlogDbContext(_options))
        {
            var handler = CreateHandler(context);
            var query = new GetRelatedArticlesQuery(baseId, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("Related", result.First().Title);
        }
    }

    [Fact]
    public async Task Handle_ShouldRankArticlesByIntersectionCount()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Base", "Base", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "tag1", "tag2", "tag3" }, ArticleCategory.Technology) { Id = baseId });
            context.Articles.Add(new Article("Low", "Baixo", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "tag1" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("High", "Alto", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "tag1", "tag2" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        // Setup cache miss
        object? cacheValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        using (var context = new BlogDbContext(_options))
        {
            var handler = CreateHandler(context);
            var query = new GetRelatedArticlesQuery(baseId, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("High", result[0].Title);
            Assert.Equal("Low", result[1].Title);
        }
    }

    [Fact]
    public async Task Handle_ShouldBeCaseInsensitiveWhenComparingTags()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Base", "Base", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "DOTNET" }, ArticleCategory.Technology) { Id = baseId });
            context.Articles.Add(new Article("Related", "Relacionado", "Content", "Conteúdo", "Summary", "Resumo", new List<string> { "dotnet" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        // Setup cache miss
        object? cacheValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        using (var context = new BlogDbContext(_options))
        {
            var handler = CreateHandler(context);
            var query = new GetRelatedArticlesQuery(baseId, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("Related", result.First().Title);
        }
    }
}