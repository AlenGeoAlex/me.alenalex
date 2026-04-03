using Bloggi.Backend.Api.Web.Infrastructure;
using Bloggi.Backend.Api.Web.Options;

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
        var bootstrapLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
        TokenSecretBootstrap.EnsureSecrets(builder.Configuration, bootstrapLogger);
        
        builder.Services.AddOptions<GoogleOAuthOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.ClientId = config["OAUTH_GOOGLE_CLIENT_ID"] ?? string.Empty;
                options.RedirectUri = config["OAUTH_GOOGLE_REDIRECT_URI"] ?? string.Empty;
                options.ClientSecret = config["OAUTH_GOOGLE_CLIENT_SECRET"] ?? string.Empty;
                options.AllowRegistration = config.GetValue("OAUTH_GOOGLE_ALLOW_REGISTRATION", false);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        builder.Services.AddOptions<S3FileServiceOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.ClientId = config["S3_CLIENT_ID"] ?? string.Empty;
                options.ClientSecret = config["S3_CLIENT_SECRET"] ?? string.Empty;
                options.Region = config["S3_REGION"] ?? "auto";
                options.Endpoint = config["S3_ENDPOINT"] ?? string.Empty;
                options.Bucket = config["S3_BUCKET"] ?? "bloggi";
                options.PublicUrl = config["S3_PUBLIC_URL"] ?? string.Empty;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        builder.Services.AddOptions<AppOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.BaseUrl = config["APP_BASE_URL"] ?? string.Empty;
                options.TemplatePath = config["APP_TEMPLATE_PATH"] ?? "/var/bloggi/templates";
                bootstrapLogger.LogInformation("Template path: {TemplatePath}", options.TemplatePath);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();       
        
        builder.Services.AddOptions<TokenOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.Secret = config["TOKEN_SECRET"]!;
                options.EncryptionKey = config["TOKEN_ENC_KEY"];
                options.Issuer = config["TOKEN_ISSUER"];
            
                if (int.TryParse(config["TOKEN_EXPIRY_HOURS"], out var expiry))
                    options.ExpiryHours = expiry;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder.Configuration;
    }
}