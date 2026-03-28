using Bloggi.Backend.Api.Web.Options;
using Microsoft.Extensions.Options;

namespace Bloggi.Backend.Api.Web.Infrastructure.Services;

using System.Text;
using ErrorOr;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

public class TokenService
{
    private readonly TokenOptions _options;
    private readonly ILogger<TokenService> _logger;
    private readonly SigningCredentials _signingCredentials;
    private readonly EncryptingCredentials? _encryptingCredentials;

    public TokenService(IOptions<TokenOptions> options, ILogger<TokenService> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        _signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        if (!string.IsNullOrWhiteSpace(_options.EncryptionKey))
        {
            var encKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.EncryptionKey));
            _encryptingCredentials = new EncryptingCredentials(encKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);
        }
    }

    public ErrorOr<string> GenerateToken(GenerateTokenRequest request)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, request.Email),
                new(BloggiClaims.CanWrite, request.CanWrite.ToString().ToLower()),
                new(BloggiClaims.DisplayName, request.DisplayName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_options.ExpiryHours),
                Issuer = _options.Issuer,
                SigningCredentials = _signingCredentials,
                EncryptingCredentials = _encryptingCredentials
            };

            var handler = new JsonWebTokenHandler();
            var token = handler.CreateToken(descriptor);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate token for user {UserId}", request.UserId);
            return Errors.Infrastructure.Token.FailedToGenerate;
        }
    }

    public ErrorOr<ClaimsPrincipal> ValidateToken(string token)
    {
        try
        {
            var handler = new JsonWebTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
                ValidateIssuer = !string.IsNullOrWhiteSpace(_options.Issuer),
                ValidIssuer = _options.Issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                TokenDecryptionKey = _encryptingCredentials is not null
                    ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.EncryptionKey!))
                    : null
            };

            var result = handler.ValidateTokenAsync(token, validationParams).GetAwaiter().GetResult();
            if (!result.IsValid)
            {
                _logger.LogWarning("Token validation failed: {Reason}", result.Exception?.Message);
                return Errors.Infrastructure.Token.FailedToValidate;
            }

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return Errors.Infrastructure.Token.FailedToValidate;
        }
    }

    public record GenerateTokenRequest(
        Guid UserId,
        string Email,
        string? DisplayName,
        bool CanWrite
    );

    private static class BloggiClaims
    {
        public const string CanWrite = "bloggi:can_write";
        public const string DisplayName = "bloggi:display_name";
    }
}