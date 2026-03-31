using Bloggi.Backend.Api.Web.Database;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Bloggi.Backend.Api.Web.Features.User.Services;

/// <summary>
/// Provides methods for handling user-related operations in the system.
/// </summary>
/// <remarks>
/// This service serves as the primary entry point for user-related functionality,
/// including fetching user details by subject or ID. It interacts with the
/// database context to perform operations on user-related data and relies on
/// logging to capture operational insights.
/// </remarks>
public class UserService(
    ILogger<UserService> logger,
    IBloggiDbContext dbContext
)
{
    /// <summary>
    /// Retrieves a user based on the unique subject identifier.
    /// </summary>
    /// <param name="subject">
    /// The unique subject identifier associated with the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional token to monitor for cancellation requests.
    /// </param>
    /// <seealso cref="Errors.User.UserNotFound"/>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <c>ErrorOr&lt;UserBySubject&gt;</c> object, where <c>UserBySubject</c> holds the user details
    /// if found, or an error otherwise.
    /// </returns>
    public async Task<ErrorOr<UserBySubject>> GetUserBySubjectAsync(string subject,
        CancellationToken cancellationToken = default)
    {
        // If single or default fails, then it's not a request issue, it should throw, I am not handling it
        var user = await dbContext.Users
            .Select(x => new { x.Id, x.GoogleSubId, x.DisplayName, x.Email, x.CanWrite })
            .SingleOrDefaultAsync(x => x.GoogleSubId == subject, cancellationToken);
            
        
        if(user is null)
            return Errors.User.UserNotFound;
        
        return new UserBySubject(
            user.Id,
            user.GoogleSubId,
            user.DisplayName,
            user.Email, 
            user.CanWrite);
    }

    /// <summary>
    /// Retrieves a user based on the unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional token to monitor for cancellation requests.
    /// </param>
    /// <seealso cref="Errors.User.UserNotFound"/>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <c>ErrorOr&lt;UserById&gt;</c> object, where <c>UserById</c> holds the user details
    /// if found, or an error otherwise.
    /// </returns>
    public async Task<ErrorOr<UserById>> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        // Same logic as above, if single or default fails, then it's not a request issue, it should throw, I am not handling it'
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if(user is null)
            return Errors.User.UserNotFound;
        
        return new UserById(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.GoogleSubId,
            user.CanWrite,
            user.CreatedAt,
            user.UpdatedAt,
            null
        );
    }

    /// <summary>
    /// Updates a user's details based on the provided update request.
    /// </summary>
    /// <param name="request">
    /// The request containing the user's updated details, including user ID, display name,
    /// avatar URL, and email address.
    /// </param>
    /// <param name="executeInstantly">
    /// A flag indicating whether the changes should be immediately persisted to the database.
    /// The default value is <c>true</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional token to monitor for cancellation requests during the update operation.
    /// </param>
    /// <seealso cref="Errors.User.UserNotFound"/>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <c>ErrorOr&lt;bool&gt;</c> indicating whether the update operation was successful,
    /// or an error in case of failure.
    /// </returns>
    public async Task<ErrorOr<bool>> UpdateUserAsync(
        UpdateUserRequest request,
        bool executeInstantly = true,
        CancellationToken cancellationToken = default
    )
    {
        var updateEntry = dbContext.Users.Update(new Api.Database.Users.User()
        {
            Id = request.Id,
            DisplayName = request.DisplayName,
            AvatarUrl = request.AvatarUrl,
            Email = request.Email
        });

        if (!executeInstantly) return true;
        
        var saveChangesAsync = await dbContext.SaveChangesAsync(cancellationToken);
        if(saveChangesAsync < 1)
            return Errors.User.UserNotFound;

        return true;
    }


    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="request">
    /// The request containing the details of the user to create, including their Google subject ID,
    /// display name, email, avatar URL, and permissions.
    /// </param>
    /// <param name="executeInstantly">
    /// A flag indicating whether the creation should be committed to the database immediately.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional token to monitor for cancellation requests.
    /// </param>
    /// <seealso cref="Errors.User.UserAlreadyExists"/>
    /// <seealso cref="Error.Unexpected"/>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <c>ErrorOr&lt;Guid&gt;</c> object, where <c>Guid</c> is the unique identifier of the newly created user
    /// if the operation succeeds, or an error otherwise.
    /// </returns>
    public async Task<ErrorOr<Guid>> CreateUserAsync(
        CreateUserRequest request,
        bool executeInstantly = true,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.CreateVersion7();
        var entityEntry = dbContext.Users.Add(new Api.Database.Users.User()
        {
            Id = id,
            AvatarUrl = request.AvatarUrl,
            DisplayName = request.DisplayName,
            Email = request.Email,
            GoogleSubId = request.GoogleSubId,
            CanWrite = request.CanWrite,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        
        if (!executeInstantly) return id;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return id;
        }
        catch (Exception ex)
        {
            if (ex is not DbUpdateException dbUpdateException)
                return Error.Unexpected($"{nameof(UserService)}.{nameof(CreateUserAsync)}", ex.Message);
            
            
            return ex.Message.Contains("ix_users_email") ? Errors.User.UserAlreadyExists : Error.Unexpected($"{nameof(UserService)}.{nameof(CreateUserAsync)}", ex.Message);
        }
    }

    /// <summary>
    /// Updates the last logged-in timestamp for a user.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the user whose last logged-in timestamp is being updated.
    /// </param>
    /// <param name="time">
    /// The timestamp indicating the last time the user logged in. If null, the value is cleared.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public async Task UpdateUserLoggedInAsync(Guid id, DateTimeOffset? time,
        CancellationToken cancellationToken = default)
    {
        // dbContext.Users.ExecuteUpdateAsync(sp =>
        // {
        //     sp.SetProperty(s => s.LastLoggedInAt, time);
        // })
    }
    
    #region Models

    public record CreateUserRequest(
        string GoogleSubId,
        string? DisplayName,
        string Email,
        string? AvatarUrl,
        bool CanWrite = false,
        DateTimeOffset? LastLoginAt = null
    );
    
    /// <summary>
    /// Represents the data required to update an existing user in the system.
    /// </summary>
    /// <remarks>
    /// This record is used to encapsulate the information needed for updating a user,
    /// including their unique identifier, optional display name, optional avatar URL,
    /// and email address. It is typically utilized by services handling user modification
    /// requests. All fields are expected to be validated before being processed.
    /// </remarks>
    public record UpdateUserRequest(
        Guid Id,
        string? DisplayName,
        string? AvatarUrl,
        string Email
    );

    /// <summary>
    /// Represents a user retrieved by a unique subject identifier.
    /// </summary>
    /// <remarks>
    /// This record is used to encapsulate user data, including the user's unique identifier, email,
    /// display name, and optional avatar URL. It is typically utilized for operations where users
    /// are fetched using a subject-based query.
    /// </remarks>
    public record UserBySubject(
        Guid Id,
        string Email,
        string? DisplayName,
        string? AvatarUrl,
        bool CanWrite
    );

    /// <summary>
    /// Represents a record containing detailed information about a user, identified by their unique ID.
    /// </summary>
    /// <remarks>
    /// This record is used primarily for operations where user identification is based on a specific GUID.
    /// It holds properties essential for describing user details such as their email, display name, avatar,
    /// subject ID, and write permissions within the system.
    /// </remarks>
    public record UserById(
        Guid Id,
        string Email,
        string? DisplayName,
        string? AvatarUrl,
        string SubjectId,
        bool CanWrite,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? LastLoginAt
    );

    #endregion
}