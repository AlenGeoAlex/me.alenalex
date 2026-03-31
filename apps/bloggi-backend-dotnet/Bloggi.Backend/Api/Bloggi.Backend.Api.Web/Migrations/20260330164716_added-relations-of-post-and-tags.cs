using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloggi.Backend.Api.Web.Database.Migrations
{
    /// <inheritdoc />
    public partial class addedrelationsofpostandtags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_post_tags_posts_post_id",
                schema: "post",
                table: "post_tags",
                column: "post_id",
                principalSchema: "post",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_post_tags_tags_tag_id",
                schema: "post",
                table: "post_tags",
                column: "tag_id",
                principalSchema: "post",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_post_tags_posts_post_id",
                schema: "post",
                table: "post_tags");

            migrationBuilder.DropForeignKey(
                name: "fk_post_tags_tags_tag_id",
                schema: "post",
                table: "post_tags");
        }
    }
}
