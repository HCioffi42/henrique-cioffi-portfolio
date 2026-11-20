using FluentAssertions;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Pins the SQL that the full-text search filter emits. PostgreSQL uses an expression
/// index only when the query expression matches the indexed expression exactly, so a
/// silent change to this SQL would leave the GIN indexes in place but unused and return
/// the search to a sequential scan. These assertions mirror the index definitions
/// created in the AddLocalizedSearchIndexes migration.
/// </summary>
public class ArticleSearchSqlShapeTests
{
    private static IQueryable<Article> ArticlesQuery()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseNpgsql("Host=localhost;Database=shape;Username=u;Password=p")
            .Options;
        return new BlogDbContext(options).Articles.AsNoTracking();
    }

    [Fact]
    public void EnglishFilter_EmitsExpressionMatchingTheEnglishIndex()
    {
        var sql = ArticleSearchService
            .ApplyFullTextFilter(ArticlesQuery(), "dotnet", isPortuguese: false)
            .ToQueryString();

        sql.Should().Contain("""to_tsvector('english', a."TitleEn" || ' ' || a."SummaryEn")""");
        sql.Should().Contain("websearch_to_tsquery('english'");
    }

    [Fact]
    public void PortugueseFilter_EmitsExpressionMatchingThePortugueseIndex()
    {
        var sql = ArticleSearchService
            .ApplyFullTextFilter(ArticlesQuery(), "dotnet", isPortuguese: true)
            .ToQueryString();

        sql.Should().Contain("""to_tsvector('portuguese', a."TitlePt" || ' ' || a."SummaryPt")""");
        sql.Should().Contain("websearch_to_tsquery('portuguese'");
    }

    [Fact]
    public void Filter_UsesNoCoalesce_SoTheIndexExpressionStaysMatchable()
    {
        var sql = ArticleSearchService
            .ApplyFullTextFilter(ArticlesQuery(), "dotnet", isPortuguese: false)
            .ToQueryString();

        sql.Should().NotContain("COALESCE");
    }
}
