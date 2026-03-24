using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Queries;

/// <summary>
/// Unit tests for the GetArticlesQueryHandler using an In-Memory database to validate logic flows.
/// </summary>
public class GetArticlesQueryHandlerTests
{
    private readonly DbContextOptions<BlogDbContext> _options;

    public GetArticlesQueryHandlerTests()
    {
        // Configures a unique in-memory database instance for each test run.
        _options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldCalculateSkipAndTakeCorrectly()
    {
        // Arrange: Seeds 10 articles into the in-memory store.
        using (var context = new BlogDbContext(_options))
        {
            for (int i = 1; i <= 10; i++)
            {
                context.Articles.Add(new Article($"Title {i}", "Content", "Summary", new List<string>(), ArticleCategory.Technology));
            }
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesQueryHandler(context);
            var query = new GetArticlesQuery(PageNumber: 2, PageSize: 3);

            // Act: Requests the second page with 3 items.
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert: Verifies that only 3 items are returned and the metadata is accurate.
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(4, result.TotalPages); // 10/3 rounded up
        }
    }

    [Fact]
    public async Task Handle_WithTagIntersection_ShouldReturnOnlyArticlesWithAllTags()
    {
        // Arrange: Seeds articles with specific tag combinations.
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Match", "Content", "Summary", new List<string> { "dotnet", "csharp" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("Partial", "Content", "Summary", new List<string> { "dotnet" }, ArticleCategory.Technology));
            context.Articles.Add(new Article("None", "Content", "Summary", new List<string> { "react" }, ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesQueryHandler(context);
            var query = new GetArticlesQuery(PageNumber: 1, PageSize: 10, Tags: new List<string> { "dotnet", "csharp" });

            // Act: Performs an intersection (AND) filter.
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert: Only the "Match" article should remain after the filter.
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

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesQueryHandler(context);
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
