namespace Me.AlenAlex.Bloggi.Database.Migrations.Constants;

public static partial class Tables
{
    public static class Author
    {
        public const string TableName = "author";
        
        public static class Columns
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string Email = "email";
            public const string Description = "description";
            public const string Github = "github";
            public const string Website = "website";
            public const string LinkedIn = "linkedin";
        }
    }
}