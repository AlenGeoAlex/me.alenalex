using System.Net;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.User.Endpoints.Auth.Logout;

internal static partial class Logout
{
    class Handler(
        ILogger<Handler> logger
        ) : EndpointWithoutRequest
    {
        public override async Task HandleAsync(CancellationToken ct)
        {
            HttpContext.Response.Cookies.Delete(Constants.Auth.StateCookieName);
            HttpContext.Response.Cookies.Delete(Constants.Auth.AccessTokenCookieName);
            HttpContext.Response.Cookies.Delete(Constants.Auth.UserInfoCookieName);
            logger.LogInformation("User logged out");
            await HttpContext.Response.SendNoContentAsync(cancellation: ct);
        }

        public override void Configure()
        {
            Delete("/logout");
            Group<AuthGroup>();
            AllowAnonymous();
            Version(1);
            Description(x =>
            {
                x.WithSummary("Logout the user");
                x.WithName("Logout");
                x.ProducesProblemDetails();
                x.Produces(204);
            });
        }
    }
}