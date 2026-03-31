using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.User.Endpoints.Auth;

public sealed class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure("auth", ed =>
        {
            ed.Description(x =>
            {
                x.WithSummary("Authentication endpoints");
                x.ProducesProblemDetails();
            });
            ed.EndpointVersion(1);
            
        });
    }
}