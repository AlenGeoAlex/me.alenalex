using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloggi.Backend.Api.Web.Migrations
{
    /// <inheritdoc />
    public partial class addpostfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post_files",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(230)", maxLength: 230, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_files_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "post",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_post_files_post_id_hash",
                schema: "post",
                table: "post_files",
                columns: new[] { "post_id", "hash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_files",
                schema: "post");
        }
    }
}
