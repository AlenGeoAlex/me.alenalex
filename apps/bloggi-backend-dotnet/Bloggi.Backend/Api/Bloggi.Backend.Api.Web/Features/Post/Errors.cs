using ErrorOr;

namespace Bloggi.Backend.Api.Web.Features.Post;

internal class Errors
{
    public static class Post
    {
        public static readonly Error PostNotFound = Error.NotFound($"{nameof(Post)}.{nameof(PostNotFound)}", "Post not found.");
    }
}