using Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Events;

public class UploadFileEventHandler(
    ILogger<UploadFileEventHandler> logger,
    IFileService fileService
    ) : IEventHandler<UploadFileEventHandler.Event>
{
    public record Event(
        byte[] File,
        string FileKey,
        string ContentType
    );


    public async Task HandleAsync(Event eventModel, CancellationToken ct)
    {
        logger.LogInformation("Received file upload event for file {FileKey}", eventModel.FileKey);
        await fileService.SaveFileAsync(
            new IFileService.SaveFileRequest(
                FileKey: eventModel.FileKey,
                FileName: eventModel.FileKey,
                ContentType: eventModel.ContentType,
                Bytes: eventModel.File
            ),
            cancellationToken: ct
        );
        logger.LogInformation("File {FileKey} saved", eventModel.FileKey);
    }
}