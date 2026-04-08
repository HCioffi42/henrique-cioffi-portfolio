using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuSitePessoal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeArticleCategoryToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // HC: Custom conversion logic to map integer Enum values to string representations securely.
            migrationBuilder.Sql(@"
                ALTER TABLE ""Articles"" 
                ALTER COLUMN ""Category"" TYPE text 
                USING CASE ""Category""
                    WHEN 1 THEN 'Technology'
                    WHEN 2 THEN 'Tutorial'
                    WHEN 3 THEN 'Life'
                    WHEN 4 THEN 'News'
                    WHEN 5 THEN 'Opinion'
                    WHEN 6 THEN 'Projects'
                    ELSE 'Technology' -- Default fallback to prevent nulls
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // HC: Reverts the string representations back to integer Enums in case of a rollback.
            migrationBuilder.Sql(@"
                ALTER TABLE ""Articles"" 
                ALTER COLUMN ""Category"" TYPE integer 
                USING CASE ""Category""
                    WHEN 'Technology' THEN 1
                    WHEN 'Tutorial' THEN 2
                    WHEN 'Life' THEN 3
                    WHEN 'News' THEN 4
                    WHEN 'Opinion' THEN 5
                    WHEN 'Projects' THEN 6
                    ELSE 1 -- Default fallback
                END;
            ");
        }
    }
}