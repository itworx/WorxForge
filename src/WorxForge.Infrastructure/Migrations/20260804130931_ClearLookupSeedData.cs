using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorxForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClearLookupSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ASSET_TYPE",
                keyColumn: "asset_type_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "ASSET_TYPE",
                keyColumn: "asset_type_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "ASSET_TYPE",
                keyColumn: "asset_type_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"));

            migrationBuilder.DeleteData(
                table: "DOMAIN",
                keyColumn: "domain_id",
                keyValue: new Guid("21111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "DOMAIN",
                keyColumn: "domain_id",
                keyValue: new Guid("21111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "DOMAIN",
                keyColumn: "domain_id",
                keyValue: new Guid("21111111-1111-1111-1111-111111111103"));

            migrationBuilder.DeleteData(
                table: "TOOL",
                keyColumn: "tool_id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "TOOL",
                keyColumn: "tool_id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "TOOL",
                keyColumn: "tool_id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111103"));

            migrationBuilder.DeleteData(
                table: "USE_CASE",
                keyColumn: "use_case_id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "USE_CASE",
                keyColumn: "use_case_id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "USE_CASE",
                keyColumn: "use_case_id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111103"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ASSET_TYPE",
                columns: new[] { "asset_type_id", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), "Prompt" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), "Skill" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), "Agent" }
                });

            migrationBuilder.InsertData(
                table: "DOMAIN",
                columns: new[] { "domain_id", "name" },
                values: new object[,]
                {
                    { new Guid("21111111-1111-1111-1111-111111111101"), "Development" },
                    { new Guid("21111111-1111-1111-1111-111111111102"), "Quality Assurance" },
                    { new Guid("21111111-1111-1111-1111-111111111103"), "DevOps" }
                });

            migrationBuilder.InsertData(
                table: "TOOL",
                columns: new[] { "tool_id", "name" },
                values: new object[,]
                {
                    { new Guid("41111111-1111-1111-1111-111111111101"), "WorxGPT" },
                    { new Guid("41111111-1111-1111-1111-111111111102"), "Claude Code" },
                    { new Guid("41111111-1111-1111-1111-111111111103"), "GitHub Copilot" }
                });

            migrationBuilder.InsertData(
                table: "USE_CASE",
                columns: new[] { "use_case_id", "name" },
                values: new object[,]
                {
                    { new Guid("31111111-1111-1111-1111-111111111101"), "Code generation" },
                    { new Guid("31111111-1111-1111-1111-111111111102"), "Code review" },
                    { new Guid("31111111-1111-1111-1111-111111111103"), "Testing" }
                });
        }
    }
}
