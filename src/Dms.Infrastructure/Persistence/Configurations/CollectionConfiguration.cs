using Dms.Core.Documents;
using Dms.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dms.Infrastructure.Persistence.Configurations;

internal sealed class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("collection");

        builder.HasKey(c => c.Id);
        // UUID v7 is created in the domain (Guid.CreateVersion7), not by the database.
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Slug).HasMaxLength(Collection.SlugMaxLength);
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.Property(c => c.Name).HasMaxLength(Collection.NameMaxLength);
        builder.Property(c => c.Description).HasMaxLength(Collection.DescriptionMaxLength);
        builder.Property(c => c.Lang).HasMaxLength(2);
        builder.Property(c => c.TsConfig).HasMaxLength(Collection.TsConfigMaxLength);
        builder.Property(c => c.DomainPack).HasMaxLength(Collection.DomainPackMaxLength);

        builder.Property(c => c.Settings)
            .HasColumnType("jsonb")
            .HasConversion(new JsonDictionaryConverter(), new JsonDictionaryComparer());
    }
}
