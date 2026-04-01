using ErrorOr;

namespace Bloggi.Backend.Api.Web.Features.Post;

internal class Errors
{
    public static class Post
    {
        public static readonly Error PostNotFound = Error.NotFound($"{nameof(Post)}.{nameof(PostNotFound)}", "Post not found.");
    }
    
    public static class PostFile
    {
        public static readonly Error PostAssociationNotFound = Error.NotFound($"{nameof(PostFile)}.{nameof(PostAssociationNotFound)}", "Post association not found.");
        public static readonly Error PostFileNotFound = Error.NotFound($"{nameof(PostFile)}.{nameof(PostFileNotFound)}", "Post file not found.");
    }
}