using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class PostRevision : IEntity, IAuditable, IDisposable
{
    public Guid Id { get; set; }
    
    public Guid PostId { get; set; }
    
    public Post Post { get; set; } = null!;
    
    public int Revision { get; set; }
    
    public string Key { get; set; } = null!;

    public JsonDocument Blocks { get; set; } = null!;
    
    public DateTimeOffset PublishedAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void Dispose()
    {
        Blocks?.Dispose();
    }
}

public class PostRevisionConfiguration : AuditableEntityConfiguration<PostRevision>
{
    public override void Configure(EntityTypeBuilder<PostRevision> builder)
    {
        base.Configure(builder);
        builder.ToTable("post_revisions", "post");
        
        builder.HasIndex(x =>  new {x.PostId, x.Revision})
            .IsUnique();
        
        builder.Property(x => x.Key)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(x => x.Blocks)
            .IsRequired();

        builder.Property(x => x.Revision)
            .IsRequired();
    }
}