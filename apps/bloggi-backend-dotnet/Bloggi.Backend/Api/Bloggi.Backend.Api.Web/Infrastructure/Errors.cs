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

        public static class Unfurl
        {
            public static readonly Error BlockedByRobots = Error.Forbidden($"{nameof(Infrastructure)}.{nameof(Unfurl)}.{nameof(BlockedByRobots)}", "Blocked by robots.txt.");
            public static readonly Error ManuallyBlocked = Error.Forbidden($"{nameof(Infrastructure)}.{nameof(Unfurl)}.{nameof(ManuallyBlocked)}", "Manually blocked.");
            public static readonly Error FailedToFetch = Error.NotFound($"{nameof(Infrastructure)}.{nameof(Unfurl)}.{nameof(FailedToFetch)}", "Failed to fetch link.");
            public static readonly Error FailedToParse = Error.NotFound($"{nameof(Infrastructure)}.{nameof(Unfurl)}.{nameof(FailedToParse)}", "Failed to parse link.");
            public static readonly Error FailedToUnfurl = Error.Failure($"{nameof(Infrastructure)}.{nameof(Unfurl)}.{nameof(FailedToUnfurl)}", "Failed to unfurl link.");
            public static readonly Error NoHeadToUnfurl = Error.NotFound($"{nameof(Infrastructure)}.{nameof(Unfurl)}.{nameof(NoHeadToUnfurl)}", "No head to unfurl.");
        }
    }
}