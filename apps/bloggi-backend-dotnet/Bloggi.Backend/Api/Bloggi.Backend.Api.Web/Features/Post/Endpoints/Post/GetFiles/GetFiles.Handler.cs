using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.GetFiles;

internal static partial class GetFiles
{
    class Handler(
        ILogger<Handler> logger,
        PostService postService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var filesResult = await postService.GetFilesOfPostAsync(req.PostId, ct);
            if(filesResult.IsError)
                return filesResult.Errors;
            
            return new Response(req.PostId, filesResult.Value.Files.Select(f => new File(f.FileId, f.Name, f.Type, f.Size, f.Hash, f.CreatedAt, f.UpdatedAt)).ToArray());
        }

        public override void Configure()
        {
            Get("{postId:guid}/files");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithSummary("Get all files associated with a post");
                x.WithName("GetPostFiles");
                x.Produces<Response>();
            });
        }
    }
}