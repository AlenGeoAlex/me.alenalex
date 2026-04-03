using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloggi.Backend.Api.Web.Migrations
{
    /// <inheritdoc />
    public partial class addposttemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "template",
                schema: "post",
                table: "posts",
                type: "text",
                nullable: false,
                defaultValue: "default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "template",
                schema: "post",
                table: "posts");
        }
    }
}
