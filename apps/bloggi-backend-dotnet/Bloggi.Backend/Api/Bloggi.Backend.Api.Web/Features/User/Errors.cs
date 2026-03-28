using ErrorOr;

namespace Bloggi.Backend.Api.Web.Features.User;

public static partial class Errors
{
    public static class Auth
    {
        public static readonly Error MissingGoogleOAuthCredentials = Error.Unexpected($"{nameof(Auth)}.{nameof(MissingGoogleOAuthCredentials)}", "Google OAuth Parameters are missing. Forgot to configure env variables?");
        public static readonly Error GoogleOAuthStateNotFound = Error.Forbidden($"{nameof(Auth)}.{nameof(GoogleOAuthStateNotFound)}", "Google OAuth State not found.");
        public static readonly Error GoogleOAuthStateMismatch = Error.Forbidden($"{nameof(Auth)}.{nameof(GoogleOAuthStateMismatch)}", "Google OAuth State mismatch.");
        public static readonly Error GoogleOAuthLoginFailed = Error.Failure($"{nameof(Auth)}.{nameof(GoogleOAuthLoginFailed)}", "Google OAuth Login failed.");
        public static readonly Error RegistrationDisabled = Error.Forbidden($"{nameof(Auth)}.{nameof(RegistrationDisabled)}", "Registration is disabled.");
    }

    public static class User
    {
        public static readonly Error UserNotFound = Error.NotFound($"{nameof(User)}.{nameof(UserNotFound)}", "User not found.");
        public static readonly Error UserCannotWrite = Error.Forbidden($"{nameof(User)}.{nameof(UserCannotWrite)}", "User cannot write.");
        public static readonly Error UserAlreadyExists = Error.Conflict($"{nameof(User)}.{nameof(UserAlreadyExists)}", "User already exists.");
    }
}