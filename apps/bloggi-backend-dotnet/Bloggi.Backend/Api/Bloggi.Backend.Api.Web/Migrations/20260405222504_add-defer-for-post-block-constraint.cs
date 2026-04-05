using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloggi.Backend.Api.Web.Migrations
{
    /// <inheritdoc />
    public partial class adddeferforpostblockconstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing non-deferrable index
            migrationBuilder.DropIndex(
                name: "ix_post_blocks_post_id_position",
                schema: "post",
                table: "post_blocks");

            // Recreate as a deferrable unique constraint
            // Must be a CONSTRAINT, not just an INDEX — only constraints can be deferrable
            migrationBuilder.Sql(@"
            ALTER TABLE post.post_blocks
            ADD CONSTRAINT ix_post_blocks_post_id_position
            UNIQUE (post_id, position)
            DEFERRABLE INITIALLY DEFERRED;
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to normal index
            migrationBuilder.Sql(@"
            ALTER TABLE post.post_blocks
            DROP CONSTRAINT ix_post_blocks_post_id_position;
        ");

            migrationBuilder.CreateIndex(
                name: "ix_post_blocks_post_id_position",
                schema: "post",
                table: "post_blocks",
                columns: new[] { "post_id", "position" },
                unique: true);
        }
    }
}
