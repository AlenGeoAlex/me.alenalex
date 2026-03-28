using FastEndpoints;
using FluentValidation;

namespace Bloggi.Backend.Api.Web.Features.User.Endpoints.Auth.Login;

internal static partial class Login
{
    /// <summary>
    /// Represents a request containing the code and state required for user authentication.
    /// </summary>
    /// <param name="Code">The authentication code provided by the OAuth provider.</param>
    /// <param name="State">The state parameter used to maintain state between the request and callback.</param>
    private record Request(
        string Code,
        string State
    );

    /// <summary>
    /// Represents a response containing authentication details and user information.
    /// </summary>
    /// <param name="Token">The JWT token generated upon successful authentication.</param>
    /// <param name="AvatarUrl">The URL of the user's avatar, if available.</param>
    /// <param name="Email">The email address associated with the authenticated user.</param>
    /// <param name="DisplayName">The display name of the authenticated user.</param>
    private  record Response(
        string? AvatarUrl,
        string Email,
        string DisplayName
    );

    private  class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .NotNull()
                .WithMessage("Code is required");
            
            RuleFor(x => x.State)
                .NotEmpty()
                .NotNull()
                .WithMessage("State is required");
        }
    }
}