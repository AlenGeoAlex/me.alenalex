using FluentMigrator.Runner;
using Me.AlenAlex.Bloggi.Database.Migrations;

namespace Me.AlenAlex.Bloggi.Api.Extensions;

public static class MigrationExtension
{
    public static Task RunMigration(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("bloggi");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Postgres connection string not found");
        }
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddPostgres().WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(DbMarker).Assembly).For.Migrations();
            });
        
        var provider = serviceCollection.BuildServiceProvider(false);
        var runner = provider.GetRequiredService<IMigrationRunner>();
        
        runner.MigrateUp();
        return Task.CompletedTask;
    } 
}