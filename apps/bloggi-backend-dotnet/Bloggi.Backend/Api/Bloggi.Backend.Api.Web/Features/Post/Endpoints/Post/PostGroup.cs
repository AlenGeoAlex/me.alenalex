using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;

public sealed class PostGroup : Group
{
    public PostGroup()
    {
        Configure("post", ed =>
        {
            ed.Description(x =>
            {
                x.WithSummary("Post endpoints");
                x.ProducesProblemDetails();
            });
            ed.EndpointVersion(1);
        });
    }
}