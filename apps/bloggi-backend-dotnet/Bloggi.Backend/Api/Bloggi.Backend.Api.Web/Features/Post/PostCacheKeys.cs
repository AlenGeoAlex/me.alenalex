namespace Bloggi.Backend.Api.Web.Features.Post;

public static class PostCacheKeys
{
    public const string PostMasterKey = "pots";
    public const string RenderCacheKey = "post-render";
    public const string PreviewCacheKey = "post-preview";
    public const string PostSummaryCacheKey = "post-summary";
    public const string PostByIdCacheKey = "post-by-id";

    public static class PostCacheTags
    {
        public const string Post = "post";
        public const string Tag = "tag";
        public const string User = "user";
        public const string PostFile = "post-file";
        public const string Meta = "meta";
        public const string Revision = "revision";
        public const string Block = "block";
        public const string Head = "head";
        
        public const string Template = "template";
    }
}