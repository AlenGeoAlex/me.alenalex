using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.ListRevisions;

internal static partial class ListRevisions
{
    class Handler(
        ILogger<Handler> logger,
        PostService postService,
        RevisionService revisionService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var revisionResult = await revisionService.GetRevisionsOfPostAsync(new RevisionService.GetRevisionsOfPost(req.PostId), ct); 
            if(revisionResult.IsError)
                return revisionResult.Errors;

            var postResponse = revisionResult.Value;
            return new Response(
                req.PostId,
                postResponse.Revisions.Select(x => new Revision(
                    x.Id,
                    x.CreatedAt,
                    x.Revision,
                    x.Key,
                    !string.IsNullOrWhiteSpace(x.Key),
                    !string.IsNullOrWhiteSpace(postResponse.CurrentPublishedKey) && x.Key == postResponse.CurrentPublishedKey,
                    x.PublishedAt
                )).ToArray()
            );
        }

        public override void Configure()
        {
            Get("/{postId:guid}/revisions");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithName("ListRevisions");
                x.WithDescription("List revisions for a post");
                x.Produces<Response>();
            });
        }
    }
}