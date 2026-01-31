using FluentMigrator;

namespace Me.AlenAlex.Bloggi.Database.Migrations.Migrations;

/// <summary>
/// Initial Migration setting up database extensions
/// </summary>
[Migration(00001)]
public class SetupMigration : Migration {
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"btree_gin\"");
    }

    public override void Down()
    {
        Execute.Sql("DROP EXTENSION IF EXISTS \"btree_gin\"");
    }
}