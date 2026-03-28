using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bloggi.Backend.Api.Web.Features.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Bloggi.Backend.Api.Web.Infrastructure;

public static class TokenSecretBootstrap
{
    private const string SecretsFileName = "bloggi-secrets.json";
    private const int SecretByteLength = 64;  // 512-bit
    private const int EncKeyByteLength = 32;  // 256-bit exactly, AES-256 requirement

    /// <summary>
    /// Call this BEFORE binding TokenOptions.
    /// Ensures TOKEN_SECRET exists — generates and persists if missing.
    /// TOKEN_ENC_KEY is optional, never auto-generated (you opt in explicitly).
    /// </summary>
    public static void EnsureSecrets(IConfigurationManager configuration, ILogger logger)
    {
        var secretsPath = ResolveSecretsPath(configuration, logger);
        var existingSecrets = LoadExistingSecrets(secretsPath);

        var secret = configuration["TOKEN_SECRET"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            secret = GenerateSecret(SecretByteLength);
            existingSecrets["TOKEN_SECRET"] = secret;

            PersistSecrets(secretsPath, existingSecrets, logger);

            // Inject into config so options binding picks it up immediately
            configuration["TOKEN_SECRET"] = secret;

            logger.LogWarning(
                "TOKEN_SECRET was not configured. A new secret has been generated and written to {SecretsPath}. " +
                "Mount this file or set the env variable explicitly to persist tokens across restarts.",
                secretsPath
            );
        }

        ValidateEncryptionKey(configuration, logger);
    }

    private static void ValidateEncryptionKey(IConfigurationManager configuration, ILogger logger)
    {
        var encKey = configuration["TOKEN_ENC_KEY"];
        if (string.IsNullOrWhiteSpace(encKey))
        {
            logger.LogInformation("TOKEN_ENC_KEY not set — JWE encryption disabled. Tokens will be signed (JWS) only.");
            return;
        }

        var keyBytes = Encoding.UTF8.GetByteCount(encKey);
        if (keyBytes != EncKeyByteLength)
        {
            // Hard fail — a wrong-length enc key is a misconfiguration, not something to silently ignore
            throw new InvalidOperationException(
                $"TOKEN_ENC_KEY must be exactly {EncKeyByteLength} UTF-8 bytes for AES-256. " +
                $"Current length: {keyBytes} bytes."
            );
        }
    }

    private static string ResolveSecretsPath(IConfigurationManager configuration, ILogger logger)
    {
        // Allow overriding the secrets directory — useful for Docker volume mounts
        // e.g. BLOGGI_SECRETS_PATH=/run/secrets
        var overridePath = configuration["SECRETS_PATH"];
        logger.LogInformation(
            !string.IsNullOrWhiteSpace(overridePath)
                ? "Using secrets path from BLOGGI_SECRETS_PATH env var: {SecretsPath}"
                : "No secrets path from BLOGGI_SECRETS_PATH env var: {SecretsPath}", overridePath);
        var directory = string.IsNullOrWhiteSpace(overridePath)
            ? AppContext.BaseDirectory
            : overridePath;

        logger.LogInformation("Secrets directory: {SecretsPath}", directory);
        return Path.Combine(directory, SecretsFileName);
    }

    private static Dictionary<string, string> LoadExistingSecrets(string path)
    {
        if (!File.Exists(path))
            return new Dictionary<string, string>();

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private static void PersistSecrets(string path, Dictionary<string, string> secrets, ILogger logger)
    {
        try
        {
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(secrets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            logger.LogInformation("Secrets written to {SecretsPath}", path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to persist secrets to {SecretsPath}. " +
                "Secret is in-memory only — tokens will be invalidated on restart.",
                path
            );
        }
    }

    private static string GenerateSecret(int byteLength)
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteLength));

    public static WebApplicationBuilder ConfigureTokenSecrets(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var secret = builder.Configuration["TOKEN_SECRET"]!;
                var encKey = builder.Configuration["TOKEN_ENC_KEY"];
                var issuer = builder.Configuration["TOKEN_ISSUER"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidIssuer = issuer,

                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    TokenDecryptionKey = !string.IsNullOrWhiteSpace(encKey)
                        ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encKey))
                        : null
                };

                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        if (!context.Request.Cookies.TryGetValue(Constants.Auth.AccessTokenCookieName,
                                out var accessToken)) return Task.CompletedTask;
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });
        builder.Services.AddAuthorization();
        
        return builder;
    }
}