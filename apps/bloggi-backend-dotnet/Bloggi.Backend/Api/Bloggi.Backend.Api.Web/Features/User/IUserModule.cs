using ErrorOr;

namespace Bloggi.Backend.Api.Web.Features.User;

public interface IUserModule
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <seealso cref="Errors.User.UserNotFound"/>
    /// <seealso cref="Error.Unexpected"/>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user information if found; otherwise, null.</returns>
    public Task<ErrorOr<UserBase>> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);


    #region UserModels

    public record UserBase(Guid Id, string Email, string? Name, string? AvatarUrl);
    
    #endregion
    
}