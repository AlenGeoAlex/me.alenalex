using Bloggi.Backend.Api.Web.Features.Post.Services;
using Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Events;

public sealed class DeletePostFileEventHandler(
    ILogger<DeletePostFileEventHandler> logger,
    IFileService fileService, 
    PostService postService
    ) : IEventHandler<DeletePostFileEventHandler.Event>
{
    public record Event(Guid PostId, Guid FileId) : IEvent;


    public async Task HandleAsync(Event eventModel, CancellationToken ct)
    {
        logger.LogInformation("Received file delete event for file {FileId} of post {PostId}", eventModel.FileId, eventModel.PostId);
        await fileService.DeleteAsync(new IFileService.DeleteFileRequest($"{eventModel.PostId}/{eventModel.FileId}"), ct);
        logger.LogInformation("File {FileId} deleted", eventModel.FileId);
        
        logger.LogInformation("Deleting file from post table {PostId}", eventModel.PostId);
        await postService.DeletePostFileAsync(new PostService.DeleteFileOfPostRequest(eventModel.PostId,
            [eventModel.FileId], true), ct);
        logger.LogInformation("File deleted from post table {PostId}", eventModel.PostId);
    }
}