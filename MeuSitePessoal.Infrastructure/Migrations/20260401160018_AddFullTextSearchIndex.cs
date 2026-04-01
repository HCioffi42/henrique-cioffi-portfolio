using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuSitePessoal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // HC: Creates a GIN (Generalized Inverted Index) for high-performance full-text search.
            // It concatenates Title and Summary into a single searchable vector using the Portuguese dictionary.
            migrationBuilder.Sql(
                @"CREATE INDEX idx_articles_search_gin ON ""Articles"" 
            USING GIN (
                (to_tsvector('english', coalesce(""Title"", '') || ' ' || coalesce(""Summary"", '')) || 
                 to_tsvector('portuguese', coalesce(""Title"", '') || ' ' || coalesce(""Summary"", ''))));"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // HC: Removes the specialized search index if the migration is rolled back.
            migrationBuilder.Sql(@"DROP INDEX idx_articles_search_gin;");
        }
    }
}
