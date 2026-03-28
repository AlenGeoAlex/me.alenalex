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

            var googleResult = await authService.ValidateAndExchangePayloadAsync(req.Code, ct);
            if (googleResult.IsError)
                return googleResult.Errors;

            var googlePayload = googleResult.Value;
            
            var userBySubjectResult = await userService.GetUserBySubjectAsync(googlePayload.Subject, ct);
            UserService.UserBySubject? user = null; 
            if (userBySubjectResult.IsError)
            {
                if (userBySubjectResult.Errors.Any(x => x.Type == ErrorType.NotFound && x.Code == Errors.User.UserNotFound.Code))
                {
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
                
                return userBySubjectResult.Errors;
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

            var token = tokenService.GenerateToken(
                new TokenService.GenerateTokenRequest(user.Id, user.Email, user.DisplayName, user.CanWrite));
            
            if (token.IsError)
                return token.Errors;
            
            return new Response(token.Value, googlePayload.Picture, googlePayload.Email, googlePayload.Name);
        }

        public override void Configure()
        {
            Post("/auth/login");
            AllowAnonymous();
            Version(1);
            Description(x =>
            {
                x.WithSummary("Login the user using the GoogleOAuth");
                x.WithName("Login");
                x.WithTags(["Auth"]);
                x.Produces<Response>(200);
                x.ProducesProblemDetails();
            });
        }
    }
}

public class TestEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/test");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        
    }
}