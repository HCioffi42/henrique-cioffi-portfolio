using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuSitePessoal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnsubscribeTokenToSubscriber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnsubscribeToken",
                table: "Subscribers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnsubscribeToken",
                table: "Subscribers");
        }
    }
}
