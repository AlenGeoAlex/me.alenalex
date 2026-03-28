using Bloggi.Backend.Api.Web.Features.User.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.User.Endpoints.Auth.GetLogin;

internal static partial class GetLogin
{
    class Handler(
        ILogger<GetLogin.Handler> logger,
        AuthService authService
        ) : EndpointWithoutRequest<ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> HandleAsync(CancellationToken ct)
        {
            var result = authService.GetGoogleOAuthLink(ct);
            if (result.IsError)
                return result.Errors;
            
            var googleOAuthLink = result.Value;
            HttpContext.Response.Cookies.Append(Constants.Auth.StateCookieName, googleOAuthLink.State, new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                MaxAge   = TimeSpan.FromMinutes(10),
                Path     = "/auth/login"
            });
            
            return new Response(googleOAuthLink.Url);
        }

        public override void Configure()
        {
            Get("/auth/login");
            AllowAnonymous();
            Version(1);
            Description(x =>
            {
                x.WithSummary("Get the login request");
                x.WithName("GetLogin");
                x.WithTags(["Auth"]);
                x.ProducesProblemDetails();
                x.Produces<Response>(200);
            });
        }
    }
}