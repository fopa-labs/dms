using Dms.Core.Documents;
using Dms.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dms.Infrastructure.Persistence.Configurations;

internal sealed class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        var status = new SnakeCaseEnumConverter<DocumentVersionStatus>();

        builder.ToTable("document_version", table =>
            // The database rejects a status the code does not know, even when written by raw SQL.
            table.HasCheckConstraint(
                "ck_document_version_status",
                $"status IN ({string.Join(", ", status.StoredValues.Select(v => $"'{v}'"))})"));

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        // Concurrent uploads of a new version compute the same number; this index lets only one win.
        builder.HasIndex(v => new { v.DocumentId, v.VersionNo }).IsUnique();

        builder.Property(v => v.BlobSha256).HasMaxLength(64).IsFixedLength();
        // Duplicate detection on upload and reference checks before deleting a blob.
        builder.HasIndex(v => v.BlobSha256);

        builder.Property(v => v.FileName).HasMaxLength(DocumentVersion.FileNameMaxLength);
        builder.Property(v => v.MimeType).HasMaxLength(DocumentVersion.MimeTypeMaxLength);
        builder.Property(v => v.Status).HasConversion(status).HasMaxLength(32);
    }
}
