using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bloggi.Backend.Api.Web.Database.DbContext;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BloggiDbContext>
{
    public BloggiDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        services.AddDbContext<BloggiDbContext>(conf =>
        {
            conf.UseNpgsql(configuration.GetConnectionString("Npgsql"), o =>
                {
                    o.SetPostgresVersion(Version.Parse("18.0"));
                    o.EnableRetryOnFailure();
                    o.MigrationsHistoryTable("bloggi_migrations", "bloggi");
                    o.UseVector();
                })
                .UseSnakeCaseNamingConvention();
        });

        return services.BuildServiceProvider().GetRequiredService<BloggiDbContext>();
    }
}