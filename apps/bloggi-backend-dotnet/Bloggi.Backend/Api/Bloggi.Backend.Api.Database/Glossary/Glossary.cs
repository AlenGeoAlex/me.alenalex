using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Glossary;

public class Glossary : IEntity, IAuditable, IDisposable
{
    public Guid Id { get; set; }
    
    public string Slug { get; private set; } = null!;
    
    public string Term { get; set; } = null!;
    
    public JsonDocument Variations { get; set; } = null!;
    
    public string? ExternalLink { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void Dispose()
    {
        Variations?.Dispose();
    }
}

public class GlossaryConfiguration : AuditableEntityConfiguration<Glossary>
{

    public override void Configure(EntityTypeBuilder<Glossary> builder)
    {
        base.Configure(builder);
        builder.ToTable("glossary", "glossary");

        builder.Property(x => x.Slug)
            .HasMaxLength(200)
            .HasComputedColumnSql(
                "lower(regexp_replace(term, '[^a-zA-Z0-9]+', '-', 'g'))",
                stored: true)
            .IsRequired();
        
        builder.HasIndex(x => x.Slug).IsUnique();
        
        builder.Property(x => x.Term).IsRequired();
        builder.Property(x => x.Variations)
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();
        
        builder.Property(x => x.ExternalLink).IsRequired(false);
    }
}