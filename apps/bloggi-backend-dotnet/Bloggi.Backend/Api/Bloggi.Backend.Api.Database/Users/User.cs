using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database.Users;

public class User : IEntity, IAuditable
{
    public Guid Id { get; set; }
    
    public string GoogleSubId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? DisplayName { get; set; }
    
    public string? AvatarUrl { get; set; }

    public bool CanWrite { get; set; } = false;
    
    public IList<Posts.Post> Posts { get; set; } = new List<Posts.Post>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class UserConfiguration : AuditableEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("users", "user");
        
        builder.HasIndex(x => x.GoogleSubId)
            .IsUnique();
        
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.GoogleSubId)
            .IsRequired();
        builder.Property(x => x.Email)
            .IsRequired();
        
        builder.Property(x => x.DisplayName)
            .IsRequired(false)
            .HasMaxLength(255);
        
        builder.Property(x => x.AvatarUrl)
            .IsRequired(false)
            .HasMaxLength(255);
        
        builder.Property(x => x.CanWrite)
            .HasDefaultValue(false);
    }
}