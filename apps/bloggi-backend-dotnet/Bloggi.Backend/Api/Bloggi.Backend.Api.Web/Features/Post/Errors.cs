using ErrorOr;

namespace Bloggi.Backend.Api.Web.Features.Post;

internal class Errors
{
    public static class Post
    {
        public static readonly Error PostNotFound = Error.NotFound($"{nameof(Post)}.{nameof(PostNotFound)}", "Post not found.");
    }

    public static class PostMeta
    {
        public static readonly Error FailedToUpdatePostMeta = Error.NotFound($"{nameof(PostMeta)}.{nameof(FailedToUpdatePostMeta)}", "Failed to update post meta.");
    }
    
    public static class PostFile
    {
        public static readonly Error PostAssociationNotFound = Error.NotFound($"{nameof(PostFile)}.{nameof(PostAssociationNotFound)}", "Post association not found.");
        public static readonly Error PostFileNotFound = Error.NotFound($"{nameof(PostFile)}.{nameof(PostFileNotFound)}", "Post file not found.");
    }
    
    public static class Revision
    {
        public static readonly Error LastRevisionYetNotPublished = Error.Conflict($"{nameof(Revision)}.{nameof(LastRevisionYetNotPublished)}", "The last revision of this post has not been published yet.");
        public static readonly Error ExistingRevisionCreationPending = Error.Conflict($"{nameof(Revision)}.{nameof(ExistingRevisionCreationPending)}", "A revision creation is already pending for this post.");
        public static readonly Error FailedToCreateRevision = Error.Failure($"{nameof(Revision)}.{nameof(FailedToCreateRevision)}", "Failed to create revision.");
        public static readonly Error NoRevisionFoundYet = Error.NotFound($"{nameof(Revision)}.{nameof(NoRevisionFoundYet)}", "No revision found for the given post and revision ID.");
    }
}