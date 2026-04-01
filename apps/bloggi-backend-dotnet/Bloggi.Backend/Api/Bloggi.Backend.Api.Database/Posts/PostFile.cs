using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class PostFile : IEntity, IAuditable
{
    public Guid Id { get; set; }
    
    public Guid? PostId { get; set; }
    
    public Post? Post { get; set; }
    
    public string Name { get; set; }
    
    public long Size { get; set; }
    
    public string ContentType { get; set; }
    
    public string Hash { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class PostFileEntityConfiguration : AuditableEntityConfiguration<PostFile>
{
    public override void Configure(EntityTypeBuilder<PostFile> builder)
    {
        base.Configure(builder);
        builder.ToTable("post_files", "post");
        
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(230);
        builder.Property(x => x.Size).IsRequired();
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Hash).IsRequired().HasMaxLength(255);
        builder.Property(x => x.PostId).IsRequired(false);
        
        
        builder.HasIndex(x => new { x.PostId, x.Hash})
            .IsUnique();

        builder.HasOne(x => x.Post)
            .WithMany(x => x.Files)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}