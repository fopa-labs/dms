using Dms.Core.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dms.Infrastructure.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tag");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        // Names are normalised (trimmed, lowercased) in the domain, so a plain unique index is enough.
        builder.Property(t => t.Name).HasMaxLength(Tag.NameMaxLength);
        builder.HasIndex(t => t.Name).IsUnique();
    }
}
