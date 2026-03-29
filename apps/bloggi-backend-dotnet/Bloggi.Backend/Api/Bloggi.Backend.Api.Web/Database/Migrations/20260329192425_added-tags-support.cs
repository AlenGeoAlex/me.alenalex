using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloggi.Backend.Api.Web.Database.Migrations
{
    /// <inheritdoc />
    public partial class addedtagssupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post_tags",
                schema: "post",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_tags", x => new { x.post_id, x.tag_id });
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    display_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_post_id",
                schema: "post",
                table: "post_tags",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_tag_id",
                schema: "post",
                table: "post_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                schema: "post",
                table: "tags",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_tags",
                schema: "post");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "post");
        }
    }
}
