using Bloggi.Backend.Api.Web.Features.User.Endpoints.Auth.Login;
using Bloggi.Backend.Api.Web.Features.User.Services;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.User.Events;

public class UserUpdateEventHandler(
    ILogger<UserUpdateEventHandler> logger,
    UserService userService
    ) : IEventHandler<UserUpdateEventHandler.Event>
{
    public record Event(
        Guid Id,
        string Email,
        string Name,
        string AvatarUrl
    );

    public async Task HandleAsync(Event eventModel, CancellationToken ct)
    {
        logger.LogInformation("Request received to update user {Id}. Updating with {Email}, {Name}, {AvatarUrl}", eventModel.Id, eventModel.Email, eventModel.Name, eventModel.AvatarUrl);
        var updateUserResult = await userService.UpdateUserAsync(new UserService.UpdateUserRequest(eventModel.Id, eventModel.Name, eventModel.AvatarUrl, eventModel.Email), cancellationToken: ct);
        if (updateUserResult.IsError)
        {
            logger.LogError("Failed to update user {Id} due to {Error}", eventModel.Id, updateUserResult.Errors.First().Description);
        }
        
        logger.LogInformation("User {Id} updated successfully", eventModel.Id);
    }
}