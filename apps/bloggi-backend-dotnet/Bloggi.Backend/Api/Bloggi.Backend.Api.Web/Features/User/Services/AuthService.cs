using System.Security.Cryptography;
using System.Web;
using Bloggi.Backend.Api.Web.Options;
using ErrorOr;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
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
    /// Exchanges a Google OAuth authorization code for an access token.
    /// </summary>
    /// <param name="code">The authorization code received from Google OAuth.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <seealso cref="Errors.Auth.InvalidOAuthJwt"/>
    /// <seealso cref="Errors.Auth.MissingGoogleOAuthCredentials"/>
    /// <returns>
    /// An ErrorOr containing the access token as a string if the exchange is successful.
    /// Returns an error if the required Google OAuth credentials are missing or if the exchange fails.
    /// </returns>
    public async Task<ErrorOr<string>> ExchangeCodeForTokenAsync(string code,
        CancellationToken cancellationToken = default)
    {
        var valueClientSecret = googleOAuthOptions.Value.ClientSecret;
        var valueClientId = googleOAuthOptions.Value.ClientId;
        if (string.IsNullOrWhiteSpace(valueClientSecret) || string.IsNullOrWhiteSpace(valueClientId))
            return Errors.Auth.MissingGoogleOAuthCredentials;
        
        try
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId     = valueClientId,
                    ClientSecret = valueClientSecret,
                }
            });

            logger.LogInformation("Exchanging Google OAuth code for token");
            var tokenResponse = await flow.ExchangeCodeForTokenAsync(
                userId: "user",
                code: code,
                redirectUri: googleOAuthOptions.Value.RedirectUri,
                taskCancellationToken: cancellationToken);

            return tokenResponse.IdToken;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to exchange Google OAuth code");
            return Errors.Auth.GoogleOAuthLoginFailed;
        }
    }

    /// <summary>
    /// Validates a Google OAuth ID token and returns its payload if the token is successfully verified.
    /// </summary>
    /// <param name="idToken">The Google OAuth ID token to be validated.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <seealso cref="Errors.Auth.InvalidOAuthJwt"/>
    /// <seealso cref="Errors.Auth.MissingGoogleOAuthCredentials"/>
    /// <returns>
    /// An ErrorOr containing the GoogleJsonWebSignature.Payload if the validation is successful.
    /// Returns an error if the token is invalid, the credentials are missing, or the validation fails.
    /// </returns>
    public async Task<ErrorOr<GoogleJsonWebSignature.Payload>> ValidateOAuthPayloadAsync(
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
            return Errors.Auth.InvalidOAuthJwt;
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