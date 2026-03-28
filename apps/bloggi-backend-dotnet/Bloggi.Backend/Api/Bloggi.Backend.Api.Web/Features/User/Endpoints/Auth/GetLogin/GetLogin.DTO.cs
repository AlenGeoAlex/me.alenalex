namespace Bloggi.Backend.Api.Web.Features.User.Endpoints.Auth.GetLogin;

internal static partial class GetLogin
{
    /// <summary>
    /// Represents the response model for the GetLogin endpoint.
    /// </summary>
    private record Response(
        string GoogleLoginUrl
    );
}