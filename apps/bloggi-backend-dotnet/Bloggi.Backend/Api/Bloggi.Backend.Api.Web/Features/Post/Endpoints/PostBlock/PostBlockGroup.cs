using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.PostBlock;

public sealed class PostBlockGroup : Group
{
    public PostBlockGroup()
    {
        Configure("/{postId:guid}/block", definition =>
        {
            definition.Description(x =>
            {
                x.WithSummary("Post block endpoints");
                x.WithGroupName("PostBlock");
            });
            definition.EndpointVersion(1);
            definition.Group<PostGroup>();
        });
    }
}