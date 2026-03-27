using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloggi.Backend.Api.Database;

public interface IAuditable
{
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
}

public abstract class AuditableEntityConfiguration<T> : EntityConfiguration<T>
    where T : class, IEntity, IAuditable
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}