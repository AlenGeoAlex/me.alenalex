using Bloggi.Backend.Api.Database.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class Post : IEntity, IAuditable
{
    
    public const string DefaultTemplate = "default";
    
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    public string Slug { get; set; } = null!;
    
    public string Title { get; set; } = null!;
    
    public string? Excerpt { get; set; }
    
    public PostStatus Status { get; set; }
    
    public string? RenderedKey { get; set; }
    
    public string Template { get; set; } = DefaultTemplate;
    
    public DateTimeOffset? PublishedAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public PostMeta Metadata { get; set; } = null!;

    public IList<PostHead> Heads { get; set; } = null!;
    
    public IList<PostBlock> Blocks { get; set; } = null!;
    
    public IList<PostRevision> Revisions { get; set; } = null!;
    
    public IList<PostTag> PostTags { get; set; } = null!;
    
    public IList<PostFile> Files { get; set; } = null!;
    
    public enum PostStatus
    {
        Published,
        Draft,
    }
}

public class PostConfiguration : AuditableEntityConfiguration<Post>
{
    public override void Configure(EntityTypeBuilder<Post> builder)
    {
        base.Configure(builder);
        builder.ToTable("posts", "post");
        
        builder.Property(x => x.Slug)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(x => x.Excerpt)
            .HasMaxLength(255)
            .IsRequired(false);
        
        builder.Property(x => x.Status)
            .HasDefaultValue(Post.PostStatus.Draft);
        
        builder.Property(x => x.RenderedKey)
            .HasMaxLength(500)
            .IsRequired(false);
        
        builder.Property(x => x.PublishedAt)
            .HasDefaultValue(null)
            .IsRequired(false);
        
        builder.HasOne(x => x.Metadata)
            .WithOne(x => x.Post)
            .HasForeignKey<PostMeta>(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(x => x.Heads)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.Blocks)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.Revisions)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PostTags)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(x => x.Template)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue(Post.DefaultTemplate);
    }
}