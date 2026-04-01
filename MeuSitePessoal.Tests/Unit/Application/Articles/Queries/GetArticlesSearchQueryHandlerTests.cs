using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Articles.Queries.GetArticlesSearch;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Queries;

/// <summary>
/// Unit tests for the GetArticlesSearchQueryHandler using an In-Memory database.
/// </summary>
public class GetArticlesSearchQueryHandlerTests
{
    private readonly DbContextOptions<BlogDbContext> _options;

    public GetArticlesSearchQueryHandlerTests()
    {
        _options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Handle_WithSearchTermInTitle_ShouldReturnMatchingArticles()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Keyword Title", "Content", "Summary", new List<string>(), ArticleCategory.Technology));
            context.Articles.Add(new Article("Other Title", "Content", "Summary", new List<string>(), ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesSearchQueryHandler(context);
            var query = new GetArticlesSearchQuery(SearchTerm: "Keyword");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Keyword Title", result.Items.First().Title);
        }
    }

    [Fact]
    public async Task Handle_WithSearchTermInSummary_ShouldReturnMatchingArticles()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("Title", "Content", "My unique summary", new List<string>(), ArticleCategory.Technology));
            context.Articles.Add(new Article("Title 2", "Content", "Common summary", new List<string>(), ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesSearchQueryHandler(context);
            var query = new GetArticlesSearchQuery(SearchTerm: "unique");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Contains("unique", result.Items.First().Summary);
        }
    }

    [Fact]
    public async Task Handle_WithSearchTermIsCaseInsensitive_ShouldReturnMatchingArticles()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("UPPERCASE", "Content", "Summary", new List<string>(), ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesSearchQueryHandler(context);
            var query = new GetArticlesSearchQuery(SearchTerm: "uppercase");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("UPPERCASE", result.Items.First().Title);
        }
    }

    [Fact]
    public async Task Handle_WithEmptySearchTerm_ShouldReturnAllArticles()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("A", "C", "S", new List<string>(), ArticleCategory.Technology));
            context.Articles.Add(new Article("B", "C", "S", new List<string>(), ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesSearchQueryHandler(context);
            var query = new GetArticlesSearchQuery(SearchTerm: "");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Items.Count);
        }
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            context.Articles.Add(new Article("A", "C", "S", new List<string>(), ArticleCategory.Technology));
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesSearchQueryHandler(context);
            var query = new GetArticlesSearchQuery(SearchTerm: "XYZ");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldApplySkipAndTake()
    {
        // Arrange
        using (var context = new BlogDbContext(_options))
        {
            for (int i = 1; i <= 5; i++)
            {
                context.Articles.Add(new Article($"Match {i}", "Content", "Summary", new List<string>(), ArticleCategory.Technology));
            }
            await context.SaveChangesAsync();
        }

        using (var context = new BlogDbContext(_options))
        {
            var handler = new GetArticlesSearchQueryHandler(context);
            var query = new GetArticlesSearchQuery(SearchTerm: "Match", PageNumber: 2, PageSize: 2);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
        }
    }
}
