using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorxForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetTitleIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "ASSET",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_title",
                table: "ASSET",
                column: "title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ASSET_title",
                table: "ASSET");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "ASSET",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);
        }
    }
}
