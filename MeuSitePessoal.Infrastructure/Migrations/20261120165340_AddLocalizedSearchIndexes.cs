using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuSitePessoal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // One GIN index per language, each over the columns that language's search reads
            // and built with the dictionary that search parses its terms with.
            //
            // The expressions deliberately omit coalesce: all four columns are NOT NULL, and
            // PostgreSQL selects an expression index only when the query expression matches
            // the indexed one. These mirror the SQL that ArticleSearchService emits, which is
            // pinned by ArticleSearchSqlShapeTests.
            migrationBuilder.Sql(
                @"CREATE INDEX idx_articles_search_en ON ""Articles""
                    USING GIN (to_tsvector('english', ""TitleEn"" || ' ' || ""SummaryEn""));");

            migrationBuilder.Sql(
                @"CREATE INDEX idx_articles_search_pt ON ""Articles""
                    USING GIN (to_tsvector('portuguese', ""TitlePt"" || ' ' || ""SummaryPt""));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_articles_search_en;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_articles_search_pt;");
        }
    }
}
