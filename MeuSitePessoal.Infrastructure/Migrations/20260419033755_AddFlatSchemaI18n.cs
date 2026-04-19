using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuSitePessoal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlatSchemaI18n : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Subscribers",
                type: "text",
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "ContentEn",
                table: "Articles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentPt",
                table: "Articles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SummaryEn",
                table: "Articles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SummaryPt",
                table: "Articles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Articles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitlePt",
                table: "Articles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // HC: Custom Data Migration SQL
            // Copies legacy content to both English and Portuguese columns as a starting point.
            migrationBuilder.Sql("UPDATE \"Articles\" SET \"TitleEn\" = \"Title\", \"SummaryEn\" = \"Summary\", \"ContentEn\" = \"Content\";");
            migrationBuilder.Sql("UPDATE \"Articles\" SET \"TitlePt\" = \"Title\", \"SummaryPt\" = \"Summary\", \"ContentPt\" = \"Content\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ContentEn",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ContentPt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "SummaryEn",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "SummaryPt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "TitlePt",
                table: "Articles");
        }
    }
}
