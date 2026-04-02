using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class PostMeta : IEntity, IAuditable, IDisposable
{
    public Guid Id { get; set; }
    
    public Guid PostId { get; set; }
    
    public Post Post { get; set; } = null!;
    
    public string? OpenGraphTitle { get; set; } = null!;
    
    public string? OpenGraphDescription { get; set; } = null!;
    
    public string? OpenGraphImageUrl { get; set; } = null!;
    
    public string? CanonicalUrl { get; set; } = null!;
    
    public string Robot { get; set; } = $"{nameof(MetaRobot.Index)},{nameof(MetaRobot.Follow)}";
    public string EditorVersion { get; set; }

    public JsonDocument? SchemaOrgJson { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public enum MetaRobot
    {
        Index =  1,
        Follow = 2,
        NoIndex = 4,
        NoFollow = 8,
    }

    public void Dispose()
    {
        SchemaOrgJson?.Dispose();
    }
}

public class PostMetaConfiguration : AuditableEntityConfiguration<PostMeta>
{
    public override void Configure(EntityTypeBuilder<PostMeta> builder)
    {
        base.Configure(builder);
        builder.ToTable("post_metas", "post");

        builder.Property(x => x.OpenGraphTitle)
            .HasMaxLength(255);

        builder.Property(x => x.OpenGraphDescription)
            .HasMaxLength(255);
        
        builder.Property(x => x.OpenGraphImageUrl)
            .HasMaxLength(255)
            .IsRequired(false);
        
        builder.Property(x => x.CanonicalUrl)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(x => x.SchemaOrgJson)
            .IsRequired(false);
        
        builder.Property(x => x.Robot)
            .HasDefaultValue($"{nameof(PostMeta.MetaRobot.Index)},{nameof(PostMeta.MetaRobot.Follow)}")
            .IsRequired();

        builder.Property(x => x.EditorVersion)
            .IsRequired();
    }
}