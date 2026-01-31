using FluentMigrator;

namespace Me.AlenAlex.Bloggi.Database.Migrations.Migrations;

[Migration(00002)]
public class AuthorTable : Migration {
    public override void Up()
    {
        Create.Table(Constants.Tables.Author.TableName)
            .WithColumn(Constants.Tables.Author.Columns.Id).AsString(36).NotNullable().PrimaryKey()
            .WithColumn(Constants.Tables.Author.Columns.Name).AsString(50).NotNullable().Unique()
            .WithColumn(Constants.Tables.Author.Columns.Description).AsString(100).Nullable()
            .WithColumn(Constants.Tables.Author.Columns.Github).AsString(50).Nullable()
            .WithColumn(Constants.Tables.Author.Columns.Website).AsString(100).Nullable()
            .WithColumn(Constants.Tables.Author.Columns.LinkedIn).AsString(50).Nullable();

    }

    public override void Down()
    {
        Delete.Table(Constants.Tables.Author.TableName);
    }
}