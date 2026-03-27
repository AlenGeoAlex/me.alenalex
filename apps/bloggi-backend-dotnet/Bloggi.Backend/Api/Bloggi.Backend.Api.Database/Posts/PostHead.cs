using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class PostHead : IEntity, IAuditable
{
    public Guid Id { get; set; }
    
    public Guid PostId { get; set; }
    
    public Post Post { get; set; } = null!;
    
    public HeadKind Kind { get; set; }
    
    public string Content { get; set; } = null!;
    
    public int Position { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public enum HeadKind
    {
        Style,
        Link,
        Script,
        Meta
    }
}

public class PostHeadConfiguration : AuditableEntityConfiguration<PostHead>
{
    public override void Configure(EntityTypeBuilder<PostHead> builder)
    {
        base.Configure(builder);
        builder.ToTable("post_heads", "post");
        
        builder.Property(x => x.Content)
            .IsRequired();
        
        builder.Property(x => x.Position)
            .IsRequired();
        
        builder.HasIndex(x =>  new {x.PostId, x.Position})
            .IsUnique();
        
        builder.Property(x => x.Kind)
            .IsRequired();
        
    }
}