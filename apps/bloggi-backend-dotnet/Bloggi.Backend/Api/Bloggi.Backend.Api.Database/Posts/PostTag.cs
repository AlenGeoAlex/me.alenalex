using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class PostTag
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid TagId { get; set; }
    public Tags Tag { get; set; } = null!;
}

public class PostTagEntityConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.ToTable("post_tags", "post");
        builder.HasKey(x => new { x.PostId, x.TagId });
        builder.HasIndex(x => x.TagId);
        builder.HasIndex(x => x.PostId);
        
        
    }
}