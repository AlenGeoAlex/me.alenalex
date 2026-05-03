using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.CreateRevision;

internal static partial class CreateRevision
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
            var revisionResult = await revisionService.CreateRevisionAsync(new RevisionService.CreateRevisionRequest(postId),
                cancellationToken: ct);
            
            if(revisionResult.IsError)
                return revisionResult.Errors;

            var revisionId = revisionResult.Value.RevisionId;
            return new Response(revisionId);
        }

        public override void Configure()
        {
            Post("/{postId:guid}/revisions");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithName("CreateRevision");
                x.WithDescription("Create a new revision for a post");
                x.Produces<Response>(201);
            });
        }
    }
}