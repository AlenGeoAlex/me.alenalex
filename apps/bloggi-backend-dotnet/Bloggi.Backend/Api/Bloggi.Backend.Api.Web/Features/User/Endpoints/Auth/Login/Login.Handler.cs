using System.Text.Json;
using Bloggi.Backend.Api.Web.Features.User.Events;
using Bloggi.Backend.Api.Web.Features.User.Services;
using Bloggi.Backend.Api.Web.Infrastructure.Services;
using Bloggi.Backend.Api.Web.Options;
using ErrorOr;
using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Bloggi.Backend.Api.Web.Features.User.Endpoints.Auth.Login;


internal static partial class Login
{
    /// <summary>
    /// Handles the login functionality for the Bloggi application.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing the login requests
    /// by processing the supplied credentials, validating the user,
    /// generating authentication tokens, and sending the response back to the client.
    /// </remarks>
    /// <param name="logger">The logger instance used for logging operational details.</param>
    /// <param name="authService">The service responsible for handling authentication-related logic.</param>
    /// <param name="userService">The service managing user-related operations and interactions.</param>
    /// <returns>An instance of <see cref="Handler"/> that processes login requests.</returns>
    class Handler(
        ILogger<Login.Handler> logger,
        IOptionsSnapshot<GoogleOAuthOptions> googleOAuthOptions,
        IOptions<TokenOptions> tokenOptions,
        AuthService authService,
        UserService userService,
        TokenService tokenService
    ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            if (!HttpContext.Request.Cookies.TryGetValue(Constants.Auth.StateCookieName, out var stateCookie))
            {
                logger.LogError("Google OAuth state cookie not found");
                return Errors.Auth.GoogleOAuthStateNotFound;
            }

            if (req.State != stateCookie)
            {
                logger.LogError("Google OAuth state mismatch. [Provided Cookie: {stateCookie}, Expected Cookie: {expectedCookie}]", req.State, stateCookie);
                return Errors.Auth.GoogleOAuthStateMismatch;
            }
            
            HttpContext.Response.Cookies.Delete(Constants.Auth.StateCookieName);
            logger.LogInformation("Google OAuth state cookie deleted");

            var codeResult = await authService.ExchangeCodeForTokenAsync(req.Code, ct);
            if (codeResult.IsError)
                return codeResult.Errors;
            
            logger.LogInformation("Google OAuth code exchanged for token");
            var googleResult = await authService.ValidateOAuthPayloadAsync(codeResult.Value, ct);
            if (googleResult.IsError)
                return googleResult.Errors;

            var googlePayload = googleResult.Value;
            
            var userBySubjectResult = await userService.GetUserBySubjectAsync(googlePayload.Subject, ct);
            UserService.UserBySubject? user = null; 
            if (userBySubjectResult.IsError)
            {
                // If the error is not - user not found, then its some other shit! return early
                if (!userBySubjectResult.Errors.Any(x =>
                        x.Type == ErrorType.NotFound && x.Code == Errors.User.UserNotFound.Code))
                    return userBySubjectResult.Errors;   
                
                // If its not found, but no registration is allowed, then return early
                if (!googleOAuthOptions.Value.AllowRegistration)
                {
                    return Errors.Auth.RegistrationDisabled;
                }

                var createUserRequest = new UserService.CreateUserRequest(
                    googlePayload.Subject,
                    googlePayload.Name,
                    googlePayload.Email,
                    googlePayload.Picture
                );
                var userCreateResult = await userService.CreateUserAsync(createUserRequest, true, ct);
                    
                if(userCreateResult.IsError)
                    return userCreateResult.Errors;

                user = new UserService.UserBySubject(
                    userCreateResult.Value,
                    googlePayload.Email,
                    googlePayload.Name,
                    googlePayload.Picture,
                    createUserRequest.CanWrite
                );
            }
            else
            {
                user = userBySubjectResult.Value;
                logger.LogInformation("User found: {UserId}, {Email}", user.Id, user.Email);
            }
            
            // If something changed, update the user (this is not high priority in terms of request, a fire and forget event is fine)
            // the current request will generate these from googlePayload, so it reflects the latest, db gets updated in time
            if (user.Email != googlePayload.Email || user.DisplayName != googlePayload.Name ||
                user.AvatarUrl != googlePayload.Picture)
            {
                logger.LogInformation("User information has changed, updating user, publishing event!");
                await PublishAsync(new UserUpdateEventHandler.Event(
                    user.Id,
                    googlePayload.Email,
                    googlePayload.Name,
                    googlePayload.Picture
                ), Mode.WaitForNone, ct);
            }

            if (!user.CanWrite)
                return Errors.Auth.InSufficientPermissions;

            var token = tokenService.GenerateToken(
                new TokenService.GenerateTokenRequest(user.Id, user.Email, user.DisplayName, user.CanWrite));
            
            if (token.IsError)
                return token.Errors;
            
            // Don't use MaxAge, so that it can be session cookie, that means, once all tab is closed, its done
            HttpContext.Response.Cookies.Append(Constants.Auth.AccessTokenCookieName, token.Value, new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                Path     = "/"
            });
            var response = new Response(googlePayload.Picture, googlePayload.Email, googlePayload.Name);
            HttpContext.Response.Cookies.Append(Constants.Auth.UserInfoCookieName, JsonSerializer.Serialize(response, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }), new CookieOptions()
            {
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                Path     = "/"
            });
            
            return response;
        }

        public override void Configure()
        {
            Post("/login");
            Group<AuthGroup>();
            AllowAnonymous();
            Version(1);
            Description(x =>
            {
                x.WithSummary("Login the user using the GoogleOAuth");
                x.WithName("Login");
                x.Produces<Response>(200);
                x.ProducesProblemDetails();
            });
        }
    }
}