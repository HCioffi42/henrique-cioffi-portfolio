using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Queries;

/// <summary>
/// Unit tests for the GetArticlesQueryHandler using an In-Memory database to validate logic flows and caching.
/// </summary>
public class GetArticlesQueryHandlerTests
{
    private readonly DbContextOptions<BlogDbContext> _options;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILanguageProvider> _languageProviderMock;

    public GetArticlesQueryHandlerTests()
    {
        // Configures a unique in-memory database instance for each test run.
        _options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _cacheMock = new Mock<IMemoryCache>();
        
        _languageProviderMock = new Mock<ILanguageProvider>();
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("en");
    }
    
    private GetArticlesQueryHandler CreateHandler(BlogDbContext context)
    {
        return new GetArticlesQueryHandler(context, _cacheMock.Object, _languageProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldCalculateSkipAndTakeCorrectly()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            for (int i = 1; i <= 10; i++)
            {
                context.Articles.Add(new Article($"Title {i}", "Content", "Summary", new List<string>(), ArticleCategory.Technology));
            }
            await context.SaveChangesAsync();
        }

        // Setup cache miss
        object? cacheValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        using (var context = new BlogDbContext(_options))
        {
            var handler = CreateHandler(context);
            var query = new GetArticlesQuery(PageNumber: 2, PageSize: 3);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(4, result.TotalPages);
        }
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var cachedItems = new List<ArticleSummaryDto> { new ArticleSummaryDto { Id = Guid.NewGuid(), Title = "Cached" } };
        var cachedResult = new PagedResult<ArticleSummaryDto>(cachedItems, 1, 1, 10);
        var version = Guid.NewGuid();
        
        // Mock version key hit
        object? versionValue = version;
        _cacheMock.Setup(x => x.TryGetValue("Articles_CacheVersion", out versionValue)).Returns(true);

        // Mock data key hit (we don't know the exact key but we can match by prefix or anything else)
        object? dataValue = cachedResult;
        _cacheMock.Setup(x => x.TryGetValue(It.Is<object>(k => k.ToString()!.StartsWith("Articles_v")), out dataValue)).Returns(true);

        using (var context = new BlogDbContext(_options))
        {
            var handler = CreateHandler(context);
            var query = new GetArticlesQuery(PageNumber: 1, PageSize: 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Same(cachedResult, result);
            Assert.Equal("Cached", result.Items.First().Title);
        }
    }

    [Fact]
    public async Task Handle_WithTagIntersection_ShouldReturnOnlyArticlesWithAllTags()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Match", "Content", "Summary", new List<string> { "dotnet", "csharp" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("Partial", "Content", "Summary", new List<string> { "dotnet" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("None", "Content", "Summary", new List<string> { "react" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        // Setup cache miss
        object? cacheValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        using (var context = new BlogDbContext(_options))
        {
            var handler = CreateHandler(context);
            var query = new GetArticlesQuery(PageNumber: 1, PageSize: 10, Tags: new List<string> { "dotnet", "csharp" });

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Match", result.Items.First().Title);
            Assert.Equal(1, result.TotalCount);
        }
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnOnlyArticlesInCategory()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Tech Article", "Content", "Summary", new List<string>(), ArticleCategory.Technology));
            context.Articles.Add(new Article("News Article", "Content", "Summary", new List<string>(), ArticleCategory.News));
            await context.SaveChangesAsync();
        }

        // Setup cache miss
        object? cacheValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        using (var context = new BlogDbContext(_options))
        {
            var handler = CreateHandler(context);
            var query = new GetArticlesQuery(PageNumber: 1, PageSize: 10, Category: ArticleCategory.News);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("News Article", result.Items.First().Title);
            Assert.Equal(ArticleCategory.News, result.Items.First().Category);
        }
    }
}
