using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Bloggi.Backend.Api.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "glossary");

            migrationBuilder.EnsureSchema(
                name: "post");

            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "glossary",
                schema: "glossary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, computedColumnSql: "lower(regexp_replace(term, '[^a-zA-Z0-9]+', '-', 'g'))", stored: true),
                    term = table.Column<string>(type: "text", nullable: false),
                    variations = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    external_link = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_glossary", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    google_sub_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    can_write = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    excerpt = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    rendered_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.id);
                    table.ForeignKey(
                        name: "fk_posts_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "post_blocks",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    block_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    block_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    block_data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    block_hash = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    block_text = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    text_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "block_text" }),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_blocks", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_blocks_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "post",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_heads",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_heads", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_heads_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "post",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_metas",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    open_graph_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    open_graph_description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    open_graph_image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    canonical_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    robot = table.Column<string>(type: "text", nullable: false, defaultValue: "Index,Follow"),
                    schema_org_json = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_metas", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_metas_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "post",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_revisions",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    blocks = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_revisions_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "post",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_glossary_slug",
                schema: "glossary",
                table: "glossary",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_blocks_block_id_post_id",
                schema: "post",
                table: "post_blocks",
                columns: new[] { "block_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_blocks_post_id_position",
                schema: "post",
                table: "post_blocks",
                columns: new[] { "post_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_blocks_text_vector",
                schema: "post",
                table: "post_blocks",
                column: "text_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_post_heads_post_id_position",
                schema: "post",
                table: "post_heads",
                columns: new[] { "post_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_metas_post_id",
                schema: "post",
                table: "post_metas",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_revisions_post_id_revision",
                schema: "post",
                table: "post_revisions",
                columns: new[] { "post_id", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_user_id",
                schema: "post",
                table: "posts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "user",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_google_sub_id",
                schema: "user",
                table: "users",
                column: "google_sub_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "glossary",
                schema: "glossary");

            migrationBuilder.DropTable(
                name: "post_blocks",
                schema: "post");

            migrationBuilder.DropTable(
                name: "post_heads",
                schema: "post");

            migrationBuilder.DropTable(
                name: "post_metas",
                schema: "post");

            migrationBuilder.DropTable(
                name: "post_revisions",
                schema: "post");

            migrationBuilder.DropTable(
                name: "posts",
                schema: "post");

            migrationBuilder.DropTable(
                name: "users",
                schema: "user");
        }
    }
}
