using System.Text;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Execute;

namespace Me.AlenAlex.Bloggi.Database.Migrations.Extensions;

public static partial class ColumnExtensions  
{

    public static IExecuteExpressionRoot CreateEnumSafe(this IExecuteExpressionRoot root, string enumName,
        params string[] values)
    {
        StringBuilder builder = new StringBuilder("DO $$ BEGIN");
        builder.AppendLine();
        builder.Append($"CREATE TYPE {enumName} AS ENUM ('{string.Join("', '", values)}'); ");
        builder.AppendLine();
        builder.Append("EXCEPTION");
        builder.AppendLine();
        builder.Append("WHEN duplicate_object THEN null;");
        builder.AppendLine();
        builder.Append("END $$;");
        
        root.Sql(builder.ToString());
        return root;
    }
}