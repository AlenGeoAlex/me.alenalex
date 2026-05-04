using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Events;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.FromExistingRevision;

internal static partial class FromExistingRevision
{
    class Handler(
        ILogger<Handler> logger
        ) : EndpointWithoutRequest
    {
        public override async Task HandleAsync(CancellationToken ct)
        {
            var postId = Route<Guid>("postId");
            var revisionId = Route<Guid>("revisionId");
            await new UpdateBlockFromRevisionEventHandler.Event(postId, revisionId)
                .PublishAsync(Mode.WaitForNone, cancellation: ct);
            
            logger.LogInformation("Revision {RevisionId} of post {PostId} created from existing revision", revisionId, postId);
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