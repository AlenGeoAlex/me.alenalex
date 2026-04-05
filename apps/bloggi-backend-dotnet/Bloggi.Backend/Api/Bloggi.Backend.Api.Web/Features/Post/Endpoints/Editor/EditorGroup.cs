using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Editor;

public sealed class EditorGroup : Group
{
    public EditorGroup()
    {
        Configure("/{postId:guid}/editor", definition =>
        {
            definition.Description(descr =>
            {
                descr.WithSummary("Editor endpoints");
                descr.WithGroupName("Editor");
            });
            definition.Group<PostGroup>();
        });
    }
}