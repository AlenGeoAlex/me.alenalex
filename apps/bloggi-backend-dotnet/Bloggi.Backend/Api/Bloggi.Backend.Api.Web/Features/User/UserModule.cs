using Bloggi.Backend.Api.Web.Features.User.Services;
using ErrorOr;

namespace Bloggi.Backend.Api.Web.Features.User;

public class UserModule(
    ILogger<UserModule> logger,
    UserService userService
    ) : IUserModule
{
    public async Task<ErrorOr<IUserModule.UserBase>> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting user {Id}", userId);
        var userByIdResult = await userService.GetUserById(userId, cancellationToken);
        // Only possible error is UserNotFound, so no need to handle it now, directly return to caller the same error
        if (userByIdResult.IsError)
        {
            logger.LogError("Failed to get user {Id} due to {Error}", userId, userByIdResult.Errors.First().Description);
            return userByIdResult.Errors;
        }
        var user = userByIdResult.Value;

        return new IUserModule.UserBase(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl
        );

    }
}