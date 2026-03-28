using Bloggi.Backend.Api.Web.Features.User.Services;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.User.Events;

public class UserLoggedInEventHandler(
    ILogger<UserLoggedInEventHandler> logger,
    UserService userService
    ) : IEventHandler<UserLoggedInEventHandler.Event>
{
    public record Event(
        Guid Id,
        DateTimeOffset? Time
    );

    public async Task HandleAsync(Event eventModel, CancellationToken ct)
    {
        DateTimeOffset? time = eventModel.Time ?? DateTimeOffset.UtcNow;
        await userService.UpdateUserLoggedInAsync(eventModel.Id, time, ct);
        logger.LogInformation("User {Id} logged in at {LoggedInAt}", eventModel.Id, time);
    }
}