using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.DeleteRevision;

internal static partial class DeleteRevision
{
    class Handler(
        ILogger<Handler> logger,
        RevisionService revisionService,
        PostService postService
        ) : EndpointWithoutRequest<ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(CancellationToken ct)
        {
            var postId = Route<Guid>("postId");
            var revisionId = Route<Guid>("revisionId");
            var deletionResult = await revisionService.DeleteRevisionAsync(new RevisionService.DeleteRevisionRequest(postId, revisionId), ct);
            if(deletionResult.IsError)
                return deletionResult.Errors;

            return new Response();
        }

        public override void Configure()
        {
            Delete("/{postId}/revisions/{revisionId}");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithName("DeleteRevision");
                x.WithDescription("Delete a revision by ID");
                x.Produces(204);
            });
        }
    }
}