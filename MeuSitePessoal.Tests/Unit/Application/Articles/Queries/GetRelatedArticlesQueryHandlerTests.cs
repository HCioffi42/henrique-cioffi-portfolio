using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Articles.Queries.GetRelatedArticles;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Queries;

/// <summary>
/// Unit tests for the GetRelatedArticlesQueryHandler using an In-Memory database.
/// </summary>
public class GetRelatedArticlesQueryHandlerTests
{
    private readonly DbContextOptions<BlogDbContext> _options;
    private readonly IMemoryCache _cache;

    public GetRelatedArticlesQueryHandlerTests()
    {
        _options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        // HC: Provides a real cache provider for the related articles logic validation.
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task Handle_WithSharedTags_ShouldReturnRelatedArticles()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            var baseArticle = new Article("Base", "Content", "Summary", new List<string> { "dotnet", "csharp" }, ArticleCategory.Technology) { Id = baseId };
            context.Articles.Add(baseArticle);
            context.Articles.Add(new Article("Related", "Content", "Summary", new List<string> { "dotnet", "testing" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("Unrelated", "Content", "Summary", new List<string> { "react" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetRelatedArticlesQueryHandler(context, _cache);
            var query = new GetRelatedArticlesQuery(ArticleId: baseId);

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
            var baseArticle = new Article("Base", "Content", "Summary", new List<string> { "a", "b", "c" }, ArticleCategory.Technology) { Id = baseId };
            context.Articles.Add(baseArticle);
            context.Articles.Add(new Article("High", "Content", "Summary", new List<string> { "a", "b" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("Low", "Content", "Summary", new List<string> { "a" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetRelatedArticlesQueryHandler(context, _cache);
            var query = new GetRelatedArticlesQuery(ArticleId: baseId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("High", result[0].Title);
            Assert.Equal("Low", result[1].Title);
        }
    }

    [Fact]
    public async Task Handle_ShouldNotIncludeTheBaseArticleItself()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            var baseArticle = new Article("Base", "Content", "Summary", new List<string> { "dotnet" }, ArticleCategory.Technology) { Id = baseId };
            context.Articles.Add(baseArticle);
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetRelatedArticlesQueryHandler(context, _cache);
            var query = new GetRelatedArticlesQuery(ArticleId: baseId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task Handle_WithNoSharedTags_ShouldReturnEmptyList()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            var baseArticle = new Article("Base", "Content", "Summary", new List<string> { "dotnet" }, ArticleCategory.Technology) { Id = baseId };
            context.Articles.Add(baseArticle);
            context.Articles.Add(new Article("Unrelated", "Content", "Summary", new List<string> { "react" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetRelatedArticlesQueryHandler(context, _cache);
            var query = new GetRelatedArticlesQuery(ArticleId: baseId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task Handle_ShouldRespectLimit()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            var baseArticle = new Article("Base", "Content", "Summary", new List<string> { "tag" }, ArticleCategory.Technology) { Id = baseId };
            context.Articles.Add(baseArticle);
            for (int i = 1; i <= 5; i++)
            {
                context.Articles.Add(new Article($"Related {i}", "Content", "Summary", new List<string> { "tag" }, ArticleCategory.Technology));
            }
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetRelatedArticlesQueryHandler(context, _cache);
            var query = new GetRelatedArticlesQuery(ArticleId: baseId, Limit: 2);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }
    }

    [Fact]
    public async Task Handle_ShouldBeCaseInsensitiveWhenComparingTags()
    {
        // Arrange
        var baseId = Guid.NewGuid();
        using (var context = new BlogDbContext(_options))
        {
            var baseArticle = new Article("Base", "Content", "Summary", new List<string> { "DOTNET" }, ArticleCategory.Technology) { Id = baseId };
            context.Articles.Add(baseArticle);
            context.Articles.Add(new Article("Related", "Content", "Summary", new List<string> { "dotnet" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetRelatedArticlesQueryHandler(context, _cache);
            var query = new GetRelatedArticlesQuery(ArticleId: baseId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("Related", result.First().Title);
        }
    }
}
