using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorxForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetFollow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASSET_FOLLOW",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSET_FOLLOW", x => new { x.user_id, x.asset_id });
                    table.ForeignKey(
                        name: "FK_ASSET_FOLLOW_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_ASSET_FOLLOW_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_FOLLOW_asset_id",
                table: "ASSET_FOLLOW",
                column: "asset_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASSET_FOLLOW");
        }
    }
}
