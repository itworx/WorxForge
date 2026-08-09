using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorxForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCommentReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REPORT_COMMENT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REPORT_COMMENT",
                columns: table => new
                {
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORT_COMMENT", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_REPORT_COMMENT_COMMENT_comment_id",
                        column: x => x.comment_id,
                        principalTable: "COMMENT",
                        principalColumn: "comment_id");
                    table.ForeignKey(
                        name: "FK_REPORT_COMMENT_REPORT_report_id",
                        column: x => x.report_id,
                        principalTable: "REPORT",
                        principalColumn: "report_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_REPORT_COMMENT_comment_id",
                table: "REPORT_COMMENT",
                column: "comment_id");
        }
    }
}
