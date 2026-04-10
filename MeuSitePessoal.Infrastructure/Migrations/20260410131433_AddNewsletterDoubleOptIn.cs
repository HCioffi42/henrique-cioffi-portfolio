using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuSitePessoal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterDoubleOptIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Subscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "Subscribers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "Subscribers",
                type: "timestamp with time zone",
                nullable: true);

            // HC: Automatically verify existing active subscribers as per requirements.
            migrationBuilder.Sql("UPDATE \"Subscribers\" SET \"IsVerified\" = true, \"VerifiedAt\" = NOW() WHERE \"IsActive\" = true;");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "Subscribers");
        }
    }
}
