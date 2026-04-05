using System.Net;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;
using Bloggi.Backend.Api.Web.Options;
using ErrorOr;
using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Editor.SaveFile;

internal static partial class SaveFile
{

    class Handler(
        ILogger<Handler> logger,
        PostService postService,
        IFileService fileService,
        IOptionsSnapshot<S3FileServiceOptions> fileServiceOptions
            ) : Endpoint<Request, ErrorOr<Response>>
    {
        
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var filePostResult = await postService.CreateFileForPostAsync(
                new PostService.CreatePostFileRequest(req.PostId, req.Name, req.ContentType, req.Size, req.Hash),
                ct: ct
            );
            
            if(filePostResult.IsError)
                return filePostResult.Errors;

            var createPostFileResponse = filePostResult.Value;
            var fileKey = $"{createPostFileResponse.PostId}/{createPostFileResponse.FileId}";
            if (createPostFileResponse.Exists)
            {
                var fileExists = await fileService.GetPublicUrlIfExistsAsync(
                    new IFileService.GetPublicUrlIfExistsRequest(fileKey),
                    cancellationToken: ct
                );

                if (fileExists is not null)
                {
                    logger.LogInformation("File {FileKey} already exists, returning public url", fileKey);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return new Response(
                        fileExists.PublicUrl, null, null
                    );
                }
                
                logger.LogInformation("File {FileKey} already exists, but seems like the file doesn't exists, Falling back to write", fileKey);
            }

            var expiry = fileServiceOptions.Value.DefaultPresignedUrlExpiry;
            var presignedUrl = await fileService.GetPresignedUrlForUploadAsync(
                new IFileService.GetPresignedUrlForUploadRequest(fileKey, req.ContentType, expiry,
                    null),
                cancellationToken: ct
            );
            
            return new Response(presignedUrl.PublicUrl, presignedUrl.PresignedUrl, DateTimeOffset.UtcNow.Add(expiry));
        }

        public override void Configure()
        {
            Post("/assets/");
            Group<EditorGroup>();
            Description(x =>
            {
                x.WithDescription("Save a file associated with a post");
                x.WithName("SaveFile");
                x.WithSummary("Save a file for a post");
                x.Produces<Response>(201);
                x.Produces<Response>(409);
            });
        }
    }
    
}