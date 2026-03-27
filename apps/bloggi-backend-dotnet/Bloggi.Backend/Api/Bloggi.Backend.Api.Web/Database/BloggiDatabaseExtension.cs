using Bloggi.Backend.Api.Web.Database.DbContext;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bloggi.Backend.Api.Web.Database;

public static class BloggiDatabaseExtension
{
    /// <summary>
    /// Configures the application to use the Bloggi database by setting up necessary services and configurations.
    /// </summary>
    /// <param name="builder">An instance of <see cref="WebApplicationBuilder"/> used to configure the application.</param>
    /// <param name="loggerFactory">The logger factor to use</param>
    /// <returns>The modified <see cref="WebApplicationBuilder"/> instance.</returns>
    public static WebApplicationBuilder UseBloggiDatabase(
        this WebApplicationBuilder builder,
        ILoggerFactory? loggerFactory
        )
    {
        var connectionString = builder.Configuration["NPGSQL_CONNECTION_STRING"] ?? builder.Configuration.GetConnectionString("Npgsql");
        if(string.IsNullOrEmpty(connectionString))
            throw new Exception("NPGSQL_CONNECTION_STRING is not set");

        Log.Debug("Using connection string: {connectionString}", connectionString);
        builder.Services.AddDbContext<IBloggiDbContext, BloggiDbContext>(conf =>
        {
            conf.UseNpgsql(connectionString, o =>
                {
                    o.SetPostgresVersion(Version.Parse("18.0"));
                    o.EnableRetryOnFailure();
                    o.MigrationsHistoryTable("bloggi_migrations", "bloggi");
                    o.UseVector();
                })
                .UseSnakeCaseNamingConvention();
            
            if(loggerFactory != null)
                conf.UseLoggerFactory(loggerFactory);

            if (builder.Environment.IsDevelopment())
            {
                conf.EnableSensitiveDataLogging();
                conf.EnableDetailedErrors();
            }
        });
        return builder;
    }
}