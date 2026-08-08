using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorxForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('COMMENT', 'updated_at') IS NULL
                BEGIN
                    ALTER TABLE [COMMENT] ADD [updated_at] datetime2 NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('COMMENT', 'updated_at') IS NOT NULL
                BEGIN
                    ALTER TABLE [COMMENT] DROP COLUMN [updated_at];
                END
                """);
        }
    }
}
