using Bloggi.Backend.Api.Web.Events;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Events;

public class UpdateBlockFromRevisionEventHandler(
    ILogger<UpdateBlockFromRevisionEventHandler> logger,
    IServiceScopeFactory serviceProvider
    ) : IEventHandler<UpdateBlockFromRevisionEventHandler.Event>
{
    public record Event(Guid PostId, Guid RevisionId) : IEvent;

    public async Task HandleAsync(Event eventModel, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var revisionService = scope.ServiceProvider.GetRequiredService<RevisionService>();
        var blockService = scope.ServiceProvider.GetRequiredService<PostBlockService>();
        var revisionResult = await revisionService.GetBlocksForRevisionAsync(new RevisionService.GetBlocksOfRevisionRequest(eventModel.PostId, eventModel.RevisionId), ct);
        if (revisionResult.IsError)
        {
            logger.LogError("Failed to get blocks for revision {RevisionId} of post {PostId} due to {Error}", eventModel.RevisionId, eventModel.PostId, revisionResult.Errors.First().Description);
            return;
        }

        var revisionBlock = revisionResult.Value;
        await blockService.SetBlockDataAsync(new PostBlockService.SetBlockDataRequest(eventModel.PostId, revisionBlock),
            ct: ct);
        logger.LogInformation("Block data for revision {RevisionId} of post {PostId} updated", eventModel.RevisionId, eventModel.PostId);
        
        await new ClearCacheEventHandler.Event(
            [
                $"{PostCacheKeys.PostMasterKey}:{eventModel.PostId}",
                $"{PostCacheKeys.RenderCacheKey}:{eventModel.PostId}"
            ], [
                $"{PostCacheKeys.PostMasterKey}:{eventModel.PostId}"
            ]
        ).PublishAsync(Mode.WaitForNone, ct);
    }
}