using Dms.Core.Documents;
using Dms.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dms.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("document");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Title).HasMaxLength(Document.TitleMaxLength);
        builder.Property(d => d.Lang).HasMaxLength(2);

        builder.Property(d => d.Metadata)
            .HasColumnType("jsonb")
            .HasConversion(new JsonDictionaryConverter(), new JsonDictionaryComparer());
        // GIN index for metadata filters (@> containment), as in the spec.
        builder.HasIndex(d => d.Metadata).HasMethod("gin");

        // Optimistic concurrency: PostgreSQL's xmin system column changes on every row update, so it
        // serves as the row version (and later as the source of the ETag) without an extra column.
        builder.Property<uint>("RowVersion").IsRowVersion();

        // Computed from DeletedAt; not a column.
        builder.Ignore(d => d.IsDeleted);

        // Cursor pagination of a collection's document list: (updated_at, id).
        builder.HasIndex(d => new { d.CollectionId, d.UpdatedAt, d.Id });

        builder.HasOne<Collection>()
            .WithMany()
            .HasForeignKey(d => d.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Versions)
            .WithOne()
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // NO ACTION is checked at the end of the statement, so a hard delete of a document can cascade
        // to its versions, including the active one, without tripping this foreign key.
        builder.HasOne<DocumentVersion>()
            .WithMany()
            .HasForeignKey(d => d.ActiveVersionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Many-to-many without a join entity class: EF Core maps document_tag as a shared-type entity.
        builder.HasMany(d => d.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "document_tag",
                right => right.HasOne<Tag>().WithMany().HasForeignKey("tag_id").OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<Document>().WithMany().HasForeignKey("document_id").OnDelete(DeleteBehavior.Cascade),
                join => join.HasKey("document_id", "tag_id"));
    }
}
