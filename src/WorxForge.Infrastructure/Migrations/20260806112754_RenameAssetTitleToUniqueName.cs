using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorxForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAssetTitleToUniqueName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "title",
                table: "ASSET");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "ASSET",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unique_name",
                table: "ASSET",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_unique_name",
                table: "ASSET",
                column: "unique_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ASSET_unique_name",
                table: "ASSET");

            migrationBuilder.DropColumn(
                name: "unique_name",
                table: "ASSET");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "ASSET",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "ASSET",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
