using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuSitePessoal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyArticleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The full-text index is built over the legacy Title and Summary columns.
            // PostgreSQL would drop it implicitly along with them; dropping it first keeps
            // that dependency explicit in the migration history.
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_articles_search_gin;");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Articles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverting restores the columns and the index, but not the text they held.
            // The columns come back nullable and empty because the original values are
            // recoverable only from the backup taken before this migration was applied.
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Articles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Articles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Articles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                @"CREATE INDEX idx_articles_search_gin ON ""Articles""
            USING GIN (
                (to_tsvector('english', coalesce(""Title"", '') || ' ' || coalesce(""Summary"", '')) ||
                 to_tsvector('portuguese', coalesce(""Title"", '') || ' ' || coalesce(""Summary"", ''))));");
        }
    }
}
