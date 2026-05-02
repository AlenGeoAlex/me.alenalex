using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.File.GetFile;

internal static partial class GetFile
{

    class Handler(
        ILogger<Handler> logger,
        IFileService fileService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var urlResponse = await fileService.GetPublicUrlIfExistsAsync(new IFileService.GetPublicUrlIfExistsRequest($"{req.PostId}/{req.FileId}"), ct);
            if (urlResponse is null)
            {
                return Errors.PostFile.PostFileNotFound;
            }
            
            return new Response(urlResponse.PublicUrl);
        }

        public override void Configure()
        {
            Get("{postId:guid}/files/{fileId:guid}");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithSummary("Get presigned url/public url of file by ID");
                x.WithName("GetFile");
                x.Produces<Response>();
            });
            
        }
    }
    
}