namespace Me.AlenAlex.Bloggi.Database.Migrations.Constants;

public partial class Tables
{
    public static class Post
    {
        public const string TableName = "post";

        public static class Columns
        {
            public const string Id = "id";
            public const string Title = "title";
            public const string AuthoredDate = "authored_date";
            public const string AuthorId = "author_id";
            public const string VectorText = "vector_text";
            public const string Published = "published";
        }
    }

    public static class PostRevisions
    {
        public const string TableName = "post_revision";

        public const string TriggerPublicId = "post_revision_public_id_trigger";
        public static class Columns
        {
            public const string Id = "id";
            public const string PostId = "post_id";
            public const string RevisionDate = "revision_date";
            public const string Published = "published";
            public const string ChangeLog = "change_log";
            public const string PublicId = "public_id";
        }
    }

    public static class PostBlock
    {
        public const string TableName = "post_block";

        public static class Columns
        {
            public const string Id = "id";
            public const string PostId = "post_id";
            public const string BlockOrdinal = "block_ordinal";
            public const string RevisionId = "revision_id";
            public const string BlockType = "block_type";
            public const string ContentData = "content_data";
            public const string VectorText = "vector_text";
        }
    }
}