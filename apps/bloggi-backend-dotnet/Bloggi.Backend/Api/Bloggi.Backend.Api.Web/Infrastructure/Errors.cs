using ErrorOr;

namespace Bloggi.Backend.Api.Web.Infrastructure;

public static partial class Errors
{
    public static class Infrastructure
    {
        public static class Token
        {
            public static readonly Error FailedToGenerate = Error.Failure($"{nameof(Infrastructure)}.{nameof(Token)}.{nameof(FailedToGenerate)}", "Failed to generate token.");
            public static readonly Error FailedToValidate = Error.Unauthorized($"{nameof(Infrastructure)}.{nameof(Token)}.{nameof(FailedToValidate)}", "Failed to validate token.");
        }
    }
}