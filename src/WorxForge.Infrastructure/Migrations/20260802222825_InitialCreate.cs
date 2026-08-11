using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorxForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASSET_TYPE",
                columns: table => new
                {
                    asset_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSET_TYPE", x => x.asset_type_id);
                });

            migrationBuilder.CreateTable(
                name: "DOMAIN",
                columns: table => new
                {
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOMAIN", x => x.domain_id);
                });

            migrationBuilder.CreateTable(
                name: "TAG",
                columns: table => new
                {
                    tag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAG", x => x.tag_id);
                });

            migrationBuilder.CreateTable(
                name: "TOOL",
                columns: table => new
                {
                    tool_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TOOL", x => x.tool_id);
                });

            migrationBuilder.CreateTable(
                name: "USE_CASE",
                columns: table => new
                {
                    use_case_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USE_CASE", x => x.use_case_id);
                });

            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email_verified = table.Column<bool>(type: "bit", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    google_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "ASSET",
                columns: table => new
                {
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    author_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    markdown_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    asset_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSET", x => x.asset_id);
                    table.ForeignKey(
                        name: "FK_ASSET_ASSET_TYPE_asset_type_id",
                        column: x => x.asset_type_id,
                        principalTable: "ASSET_TYPE",
                        principalColumn: "asset_type_id");
                    table.ForeignKey(
                        name: "FK_ASSET_DOMAIN_domain_id",
                        column: x => x.domain_id,
                        principalTable: "DOMAIN",
                        principalColumn: "domain_id");
                    table.ForeignKey(
                        name: "FK_ASSET_USER_author_id",
                        column: x => x.author_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_ASSET_USE_CASE_use_case_id",
                        column: x => x.use_case_id,
                        principalTable: "USE_CASE",
                        principalColumn: "use_case_id");
                });

            migrationBuilder.CreateTable(
                name: "AUTH_SESSION",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUTH_SESSION", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_AUTH_SESSION_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "REPORT",
                columns: table => new
                {
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reporter_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resolved_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORT", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_REPORT_USER_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_REPORT_USER_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "ASSET_TAG",
                columns: table => new
                {
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSET_TAG", x => new { x.asset_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_ASSET_TAG_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_ASSET_TAG_TAG_tag_id",
                        column: x => x.tag_id,
                        principalTable: "TAG",
                        principalColumn: "tag_id");
                });

            migrationBuilder.CreateTable(
                name: "ASSET_TOOL",
                columns: table => new
                {
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tool_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSET_TOOL", x => new { x.asset_id, x.tool_id });
                    table.ForeignKey(
                        name: "FK_ASSET_TOOL_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_ASSET_TOOL_TOOL_tool_id",
                        column: x => x.tool_id,
                        principalTable: "TOOL",
                        principalColumn: "tool_id");
                });

            migrationBuilder.CreateTable(
                name: "ASSET_VIEW",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    last_viewed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    view_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSET_VIEW", x => new { x.user_id, x.asset_id });
                    table.ForeignKey(
                        name: "FK_ASSET_VIEW_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_ASSET_VIEW_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "BOOKMARK",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOOKMARK", x => new { x.user_id, x.asset_id });
                    table.ForeignKey(
                        name: "FK_BOOKMARK_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_BOOKMARK_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "COMMENT",
                columns: table => new
                {
                    comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMMENT", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_COMMENT_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_COMMENT_COMMENT_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "COMMENT",
                        principalColumn: "comment_id");
                    table.ForeignKey(
                        name: "FK_COMMENT_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "REACTION",
                columns: table => new
                {
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reaction_type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REACTION", x => new { x.asset_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_REACTION_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_REACTION_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "REPORT_ASSET",
                columns: table => new
                {
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REPORT_ASSET", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_REPORT_ASSET_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_REPORT_ASSET_REPORT_report_id",
                        column: x => x.report_id,
                        principalTable: "REPORT",
                        principalColumn: "report_id");
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATION",
                columns: table => new
                {
                    notification_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_ASSET_asset_id",
                        column: x => x.asset_id,
                        principalTable: "ASSET",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_COMMENT_comment_id",
                        column: x => x.comment_id,
                        principalTable: "COMMENT",
                        principalColumn: "comment_id");
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_REPORT_report_id",
                        column: x => x.report_id,
                        principalTable: "REPORT",
                        principalColumn: "report_id");
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_USER_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_USER_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_asset_type_id",
                table: "ASSET",
                column: "asset_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_author_id",
                table: "ASSET",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_domain_id",
                table: "ASSET",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_use_case_id",
                table: "ASSET",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_TAG_tag_id",
                table: "ASSET_TAG",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_TOOL_tool_id",
                table: "ASSET_TOOL",
                column: "tool_id");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_TYPE_name",
                table: "ASSET_TYPE",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_VIEW_asset_id",
                table: "ASSET_VIEW",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_AUTH_SESSION_user_id",
                table: "AUTH_SESSION",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKMARK_asset_id",
                table: "BOOKMARK",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_COMMENT_asset_id",
                table: "COMMENT",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_COMMENT_parent_comment_id",
                table: "COMMENT",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_COMMENT_user_id",
                table: "COMMENT",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_DOMAIN_name",
                table: "DOMAIN",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_actor_user_id",
                table: "NOTIFICATION",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_asset_id",
                table: "NOTIFICATION",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_comment_id",
                table: "NOTIFICATION",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_recipient_user_id",
                table: "NOTIFICATION",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_report_id",
                table: "NOTIFICATION",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_REACTION_user_id",
                table: "REACTION",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_REPORT_reporter_user_id",
                table: "REPORT",
                column: "reporter_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_REPORT_resolved_by",
                table: "REPORT",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "IX_REPORT_ASSET_asset_id",
                table: "REPORT_ASSET",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_REPORT_COMMENT_comment_id",
                table: "REPORT_COMMENT",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_TAG_name",
                table: "TAG",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TOOL_name",
                table: "TOOL",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USE_CASE_name",
                table: "USE_CASE",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASSET_TAG");

            migrationBuilder.DropTable(
                name: "ASSET_TOOL");

            migrationBuilder.DropTable(
                name: "ASSET_VIEW");

            migrationBuilder.DropTable(
                name: "AUTH_SESSION");

            migrationBuilder.DropTable(
                name: "BOOKMARK");

            migrationBuilder.DropTable(
                name: "NOTIFICATION");

            migrationBuilder.DropTable(
                name: "REACTION");

            migrationBuilder.DropTable(
                name: "REPORT_ASSET");

            migrationBuilder.DropTable(
                name: "REPORT_COMMENT");

            migrationBuilder.DropTable(
                name: "TAG");

            migrationBuilder.DropTable(
                name: "TOOL");

            migrationBuilder.DropTable(
                name: "COMMENT");

            migrationBuilder.DropTable(
                name: "REPORT");

            migrationBuilder.DropTable(
                name: "ASSET");

            migrationBuilder.DropTable(
                name: "ASSET_TYPE");

            migrationBuilder.DropTable(
                name: "DOMAIN");

            migrationBuilder.DropTable(
                name: "USER");

            migrationBuilder.DropTable(
                name: "USE_CASE");
        }
    }
}
