namespace Bloggi.Backend.Api.Web.Extensions;

public static class ConfigurationExtension
{
    /// <summary>
    /// Initializes the application's configuration by setting up environment variables and JSON file sources.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance used to configure the application.</param>
    /// <returns>Returns the configured IConfigurationManager instance.</returns>
    public static IConfigurationManager InitializeConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddEnvironmentVariables("BLOGGI_");
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        return builder.Configuration;
    }
}