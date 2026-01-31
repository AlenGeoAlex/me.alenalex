using FluentMigrator;
using Me.AlenAlex.Bloggi.Database.Migrations.Extensions;

namespace Me.AlenAlex.Bloggi.Database.Migrations.Migrations;

[Migration(00003)]
public class PostTables : Migration {
    public override void Up()
    {
        Create.Table(Constants.Tables.Post.TableName)
            .WithDescription("Table holding the posts")
            .WithColumn(Constants.Tables.Post.Columns.Id).AsString(36).PrimaryKey().NotNullable()
            .WithColumn(Constants.Tables.Post.Columns.Title).AsString(200).NotNullable()
            .WithColumn(Constants.Tables.Post.Columns.AuthoredDate).AsDateTimeOffset().NotNullable()
            .WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn(Constants.Tables.Post.Columns.AuthorId).AsString(36).NotNullable()
            .WithColumn(Constants.Tables.Post.Columns.Published).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(Constants.Tables.Post.Columns.VectorText).AsCustom("tsvector")
            .Computed("to_tsvector('english', coalesce(title, ''))");

        Create.Index($"gin_idx_{Constants.Tables.Post.TableName}")
            .OnTable(Constants.Tables.Post.TableName)
            .OnColumn(Constants.Tables.Post.Columns.VectorText);

        Create.Table(Constants.Tables.PostRevisions.TableName)
            .WithDescription("Revisions of a post")
            .WithColumn(Constants.Tables.PostRevisions.Columns.Id).AsString(26).NotNullable().PrimaryKey()
            .WithColumn(Constants.Tables.PostRevisions.Columns.PostId).AsString(36).NotNullable()
            .WithColumn(Constants.Tables.PostRevisions.Columns.Published).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(Constants.Tables.PostRevisions.Columns.ChangeLog).AsString().Nullable()
            .WithColumn(Constants.Tables.PostRevisions.Columns.RevisionDate).AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn(Constants.Tables.PostRevisions.Columns.PublicId).AsInt32().NotNullable();
        
        Execute.Sql($"""
                               CREATE OR REPLACE FUNCTION {Constants.Tables.PostRevisions.TriggerPublicId}()
                               RETURNS trigger AS $$
                               BEGIN
                                   IF NEW.public_id IS NULL THEN
                                       SELECT COALESCE(MAX(public_id), 0) + 1
                                       INTO NEW.public_id
                                       FROM {Constants.Tables.PostRevisions.TableName}
                                       WHERE post_id = NEW.post_id;
                                   END IF;
                               
                                   RETURN NEW;
                               END;
                               $$ LANGUAGE plpgsql;
                               """);
        
        Execute.Sql($"CREATE TRIGGER trg_post_revision_public_id BEFORE INSERT ON public.{Constants.Tables.PostRevisions.TableName} FOR EACH ROW EXECUTE FUNCTION {Constants.Tables.PostRevisions.TriggerPublicId}();");
        Execute.CreateEnumSafe(Constants.General.EnumPostBlockType, "text", "markdown", "media", "code", "html", "card_content", "embed");
        Create.Table(Constants.Tables.PostBlock.TableName)
            .WithDescription("Table holding the blocks of a post with their content and type")
            .WithColumn(Constants.Tables.PostBlock.Columns.Id).AsString(26).PrimaryKey().NotNullable()
            .WithColumn(Constants.Tables.PostBlock.Columns.BlockOrdinal).AsInt32().NotNullable()
            .WithColumn(Constants.Tables.PostBlock.Columns.BlockType).AsCustom(Constants.General.EnumPostBlockType)
            .NotNullable()
            .WithColumn(Constants.Tables.PostBlock.Columns.PostId).AsString(36).NotNullable()
            .WithColumn(Constants.Tables.PostBlock.Columns.ContentData).AsCustom("jsonb").NotNullable()
            .WithDefaultValue("{}")
            .WithColumn(Constants.Tables.PostBlock.Columns.RevisionId).AsString(26).NotNullable()
            .WithColumn(Constants.Tables.Post.Columns.VectorText).AsCustom("tsvector").Nullable();

        Create.Index($"gin_idx_{Constants.Tables.PostBlock.TableName}")
            .OnTable(Constants.Tables.PostBlock.TableName)
            .OnColumn(Constants.Tables.PostBlock.Columns.VectorText);
        
        // Relations
        Create.ForeignKey("fk_post_author_id")
            .FromTable(Constants.Tables.Post.TableName)
            .ForeignColumn(Constants.Tables.Author.Columns.Id)
            .ToTable(Constants.Tables.Author.TableName)
            .PrimaryColumn(Constants.Tables.Author.Columns.Id);
        
        Create.ForeignKey("fk_post_block_revision_id")
            .FromTable(Constants.Tables.PostBlock.TableName)
            .ForeignColumn(Constants.Tables.PostBlock.Columns.RevisionId)
            .ToTable(Constants.Tables.PostRevisions.TableName)
            .PrimaryColumn(Constants.Tables.PostRevisions.Columns.Id);
        
        Create.ForeignKey("fk_post_block_post_id")
            .FromTable(Constants.Tables.PostBlock.TableName)
            .ForeignColumn(Constants.Tables.PostBlock.Columns.PostId)
            .ToTable(Constants.Tables.Post.TableName)
            .PrimaryColumn(Constants.Tables.Post.Columns.Id);
        
        //Unique Keys
        Create.UniqueConstraint("uq_post_block_post_id_revision_id_ordinal")
            .OnTable(Constants.Tables.PostBlock.TableName)
            .Columns(
                Constants.Tables.PostBlock.Columns.PostId,
                Constants.Tables.PostBlock.Columns.RevisionId,
                Constants.Tables.PostBlock.Columns.BlockOrdinal
            );

        Create.UniqueConstraint("uq_post_revision_public_id_post_id")
            .OnTable(Constants.Tables.PostRevisions.TableName)
            .Columns(
                Constants.Tables.PostRevisions.Columns.PostId,
                Constants.Tables.PostRevisions.Columns.PublicId
            );
    }

    public override void Down()
    {
        Execute.Sql("DROP TRIGGER IF EXISTS trg_post_revision_public_id ON post_revisions;");
        Execute.Sql($"DROP FUNCTION IF EXISTS {Constants.Tables.PostRevisions.TriggerPublicId}();");
        Delete.Index($"gin_idx_{Constants.Tables.PostBlock.TableName}").OnTable(Constants.Tables.PostBlock.TableName);
        Delete.Index($"gin_idx_{Constants.Tables.Post.TableName}").OnTable(Constants.Tables.Post.TableName);
        Delete.ForeignKey("fk_post_author_id");
        Delete.ForeignKey("fk_post_block_revision_id");
        Delete.ForeignKey("fk_post_block_post_id");
        Delete.UniqueConstraint("uq_post_block_post_id_revision_id_ordinal");
        Delete.UniqueConstraint("uq_post_revision_public_id_post_id");
        
        Delete.Table(Constants.Tables.PostBlock.TableName);
        Delete.Table(Constants.Tables.PostRevisions.TableName);
        Delete.Table(Constants.Tables.Post.TableName);
    }
}