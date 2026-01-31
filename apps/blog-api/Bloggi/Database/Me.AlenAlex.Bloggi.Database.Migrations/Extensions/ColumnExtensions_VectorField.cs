using System.Text;
using FluentMigrator.Builders.Create.Table;

namespace Me.AlenAlex.Bloggi.Database.Migrations.Extensions;

public static partial class ColumnExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax WithVectorField(
        this ICreateTableWithColumnSyntax table,
        string indexName,
        params string[] columnNames
    )
    {
        ArgumentNullException.ThrowIfNull(indexName);
        
        if(columnNames.Length == 0)
            throw new Exception($"No vector fields defined for {indexName}.");
        
        StringBuilder queryBuilder = new StringBuilder();
        queryBuilder.Append($"to_tsvector('english', ");
        queryBuilder.AppendLine();
        for (var i = 0; i < columnNames.Length; i++)
        {
            var columnName = columnNames[i];
            queryBuilder.Append($"coalesce({columnName}, '') ");
            if(i != columnNames.Length - 1)
                queryBuilder.Append("|| ' ' || ");
        }
        
        queryBuilder.AppendLine();
        queryBuilder.Append(")");
        return table
            .WithColumn("vector_text")
            .AsCustom(queryBuilder.ToString())
            .NotNullable();
    }
}