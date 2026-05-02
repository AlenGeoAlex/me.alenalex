using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.File.DeleteFile;

internal static partial class DeleteFile
{
    class Handler(
        ILogger<Handler> logger,
        PostService postService
        ) : Endpoint<Request>
    {
        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            var response =
                await postService.DeletePostFileAsync(
                    new PostService.DeleteFileOfPostRequest(req.PostId, [req.FileId]), ct);

            if (response.IsError || !response.Value)
            {
                logger.LogWarning("Failed to delete file {FileId} for post {PostId}", req.FileId, req.PostId);
                await HttpContext.Response.SendErrorsAsync([], cancellation: ct);
                return;
            }
            
            await HttpContext.Response.SendNoContentAsync(ct);
        }

        public override void Configure()
        {
            Delete("/{postId:guid}/files/{fileId:guid}");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithSummary("Delete a file by ID");
                x.WithName("DeleteFile");
                x.Produces(204);
            });
        }
    }
}