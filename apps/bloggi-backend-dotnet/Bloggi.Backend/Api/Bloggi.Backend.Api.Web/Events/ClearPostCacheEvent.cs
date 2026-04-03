using FastEndpoints;
using ZiggyCreatures.Caching.Fusion;

namespace Bloggi.Backend.Api.Web.Events;

public class ClearCacheEventHandler(
    ILogger<ClearCacheEventHandler> logger,
    IFusionCache cache
    ) : IEventHandler<ClearCacheEventHandler.Event>
{
    public record Event(
        string[]? Keys,
        string[]? Tags
    ) : IEvent;

    public async Task HandleAsync(Event eventModel, CancellationToken ct)
    {
        var keys = eventModel.Keys ?? Array.Empty<string>();
        var tags = eventModel.Tags ?? Array.Empty<string>();
        List<Task> tasks = keys.Select(x => cache.RemoveAsync($"{x}*", token: ct).AsTask()).ToList();
        tasks.AddRange(tags.Select(x => cache.RemoveByTagAsync(x, token: ct).AsTask()));
        logger.LogInformation("Clearing cache for {IdsCount} and {TagsCount} keys and tags", keys.Length, tags.Length);
        await Task.WhenAll(tasks);
    }
}