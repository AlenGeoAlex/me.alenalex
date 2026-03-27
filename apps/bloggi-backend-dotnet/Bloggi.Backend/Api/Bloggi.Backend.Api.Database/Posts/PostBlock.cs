using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class PostBlock : IEntity, IAuditable, IDisposable
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; }
    public string BlockId { get; set; } = null!;
    public string BlockType { get; set; } = null!;
    public int Position { get; set; }
    public JsonDocument BlockData { get; set; } = null!;
    public string BlockHash {get; set;} = null!;
    public string? BlockText { get; set; }
    public NpgsqlTypes.NpgsqlTsVector TextVector { get; set; } = null!;
        
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void Dispose()
    {
        BlockData.Dispose();
    }
}

public class PostBlockConfiguration : AuditableEntityConfiguration<PostBlock>
{
    public override void Configure(EntityTypeBuilder<PostBlock> builder)
    {
        base.Configure(builder);
        builder.ToTable("post_blocks", "post");
        builder.Property(x => x.BlockId)
            .HasMaxLength(20)
            .IsRequired();
        
        builder.HasIndex(x => new { x.BlockId, x.PostId })
            .IsUnique();
        
        builder.Property(x => x.BlockType)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.Position)
            .IsRequired();
        
        builder.HasIndex(x => new
        {
            x.PostId,
            x.Position
        }).IsUnique();
        
        builder.Property(x => x.BlockData)
            .IsRequired();
        
        builder.Property(x => x.BlockHash)
            .HasMaxLength(150)
            .IsRequired();
        
        builder.Property(x => x.BlockText)
            .HasColumnType("text")
            .HasDefaultValue("")
            .IsRequired(true);

        builder.HasGeneratedTsVectorColumn(x => x.TextVector,
                "english",
                x => new { x.BlockText }
            )
            .HasIndex(x => x.TextVector)
            .HasMethod("GIN");
    }
}