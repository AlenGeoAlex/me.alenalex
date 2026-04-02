using System.Net;
using System.Security.Cryptography;
using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Events;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.File.SaveUrl;

internal static partial class SaveUrl
{
    class Handler(
        ILogger<Handler> logger,
        IHttpClientFactory httpClientFactory,
        PostService postService,
        IFileService fileService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var httpClient = httpClientFactory.CreateClient();
            using var fetchResponse = await httpClient.GetAsync(req.Url, ct);
            
            if(fetchResponse.StatusCode == HttpStatusCode.NotFound)
                return Error.Failure("RemoteFile.NotFound", "The remote file was not found");
            
            if(!fetchResponse.IsSuccessStatusCode)
                return Error.Failure("RemoteFile.Invalid", "The remote file was not found");
            
            var contentType = fetchResponse.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            if(!contentType.StartsWith("image/"))
                return Error.Failure("RemoteFile.Invalid", "Only images are allowed");
            
            var filename = req.Url.Split('/').Last().Split('?').First();
            if (string.IsNullOrWhiteSpace(filename)) filename = "image";
            
            var bytes = await fetchResponse.Content.ReadAsByteArrayAsync(ct);
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLower();

            var createFileResponse = await postService.CreateFileForPostAsync(new PostService.CreatePostFileRequest(
                req.PostId,
                filename,
                contentType,
                bytes.Length,
                hash
            ), ct);
            
            if(createFileResponse.IsError)
                return createFileResponse.Errors;

            var createPostFileResponse = createFileResponse.Value;
            var fileKey = $"{createPostFileResponse.PostId}/{createPostFileResponse.FileId}";
            if (createPostFileResponse.Exists)
            {
                var existsResponse = await fileService.GetPublicUrlIfExistsAsync(new IFileService.GetPublicUrlIfExistsRequest(fileKey), ct);
                if (existsResponse is not null)
                {
                    logger.LogInformation("File {FileKey} already exists, returning public url", fileKey);
                    return new Response(existsResponse.PublicUrl);
                }
                
                logger.LogInformation("File {FileKey} already exists, but seems like the file doesn't exists, Falling back to write", fileKey);
            }

            await PublishAsync(new UploadFileEventHandler.Event(bytes, fileKey, contentType), Mode.WaitForNone, ct);
            return new Response(fileService.BuildPublicUrl(fileKey));
        }

        public override void Configure()
        {
            Post("/{postId:guid}/file-url");
            Description(x =>
            {
                x.WithDescription("Save a file associated with a post");
                x.WithName("SaveUrl");
                x.WithSummary("Save a file for a post");
                x.Produces<Response>(201);
            });
            Version(1);
            Group<PostGroup>();
        }
    }
}