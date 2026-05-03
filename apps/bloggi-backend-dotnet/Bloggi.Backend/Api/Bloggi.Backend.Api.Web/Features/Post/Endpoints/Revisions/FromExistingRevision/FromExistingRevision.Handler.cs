using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Events;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.FromExistingRevision;

internal static partial class FromExistingRevision
{
    class Handler(
        ILogger<Handler> logger
        ) : Endpoint<Request>
    {
        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            await new UpdateBlockFromRevisionEventHandler.Event(req.PostId, req.RevisionId)
                .PublishAsync(Mode.WaitForNone, cancellation: ct);
            
            logger.LogInformation("Revision {RevisionId} of post {PostId} created from existing revision", req.RevisionId, req.PostId);
        }

        public override void Configure()
        {
            Post("{postId:guid}/revisions/{revisionId:guid}/from-existing");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithName("FromExistingRevision");
                x.WithDescription("Create a new revision from an existing revision");
                x.Produces(200);
            });
        }
    }
}