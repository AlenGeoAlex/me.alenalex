using System.Security.Cryptography;
using System.Web;
using Bloggi.Backend.Api.Web.Options;
using ErrorOr;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Bloggi.Backend.Api.Web.Features.User.Services;

/// <summary>
/// A service for handling user authentication-related operations, including generating OAuth login URLs
/// and validating OAuth tokens.
/// </summary>
public class AuthService(
    IOptionsSnapshot<GoogleOAuthOptions> googleOAuthOptions,
    ILogger<AuthService> logger
)
{
    /// <summary>
    /// Generates a Google OAuth login URL along with a state parameter and returns it as a structured response.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <seealso cref="Errors.Auth.MissingGoogleOAuthCredentials"/>
    /// <returns>
    /// An ErrorOr containing a GoogleOAuthUrlState which includes the constructed URL and state parameter.
    /// Returns an error if required Google OAuth credentials are missing.
    /// </returns>
    public ErrorOr<GoogleOAuthUrlState> GetGoogleOAuthLink(CancellationToken cancellationToken = default)
    {
        var googleClientId = googleOAuthOptions.Value.ClientId;
        var redirectUri = googleOAuthOptions.Value.RedirectUri;
        if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(redirectUri))
            return Errors.Auth.MissingGoogleOAuthCredentials;
        
        var state = GenerateGoogleOAuthState();
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["client_id"]     = googleClientId;
        queryParams["redirect_uri"]  = redirectUri;
        queryParams["response_type"] = "code";
        queryParams["scope"]         = "openid email profile";
        queryParams["state"]         = state;
        queryParams["access_type"]   = "offline";
        queryParams["prompt"]        = "consent";

        var url = $"https://accounts.google.com/o/oauth2/v2/auth?{queryParams}";
        return new GoogleOAuthUrlState(url, state);
    }

    /// <summary>
    /// Validates a Google ID token, exchanges it for its payload, and returns the result.
    /// </summary>
    /// <param name="idToken">The Google ID token to validate and exchange.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <seealso cref="Errors.Auth.MissingGoogleOAuthCredentials"/>
    /// <seealso cref="Errors.Auth.GoogleOAuthLoginFailed"/>
    /// <returns>
    /// An ErrorOr containing the GoogleJsonWebSignature.Payload if the token is successfully validated.
    /// Returns an error if the token is invalid or if required Google OAuth credentials are missing.
    /// </returns>
    public async Task<ErrorOr<GoogleJsonWebSignature.Payload>> ValidateAndExchangePayloadAsync(
        string idToken,
        CancellationToken cancellationToken = default
        )
    {
        var googleClientId = googleOAuthOptions.Value.ClientId;
        if (string.IsNullOrWhiteSpace(googleClientId))
            return Errors.Auth.MissingGoogleOAuthCredentials;
        
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [googleClientId]
        };

        try
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return payload;
        }
        catch (InvalidJwtException exception)
        {
            logger.LogError(exception, "Failed to validate Google JWT");
            return Errors.Auth.GoogleOAuthLoginFailed;
        }
    }

    
    #region PrivateHelperMethods

    private static string GenerateGoogleOAuthState()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    #endregion
    
    #region Models

    public record GenerateAccessTokenRequest(
        string GoogleSubId,
        string DisplayName,
        string Email
    );
    
    public record BloggiCredentials(
        string AccessToken,
        string RefreshToken //Not for now
    );

    public record GoogleOAuthUrlState(
        string Url,
        string State
    );

    #endregion
}