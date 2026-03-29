using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Posts;

public class Tags : IEntity
{
    public Guid Id { get; set; }
    
    public string Slug { get; set; }
    
    public string DisplayName { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}

public class TagsEntityConfiguration : EntityConfiguration<Tags>
{
    public override void Configure(EntityTypeBuilder<Tags> builder)
    {
        base.Configure(builder);
        builder.ToTable("tags", "post");
        
        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(60);
        
        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}