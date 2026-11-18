using Dms.Core.Auditing;
using Dms.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dms.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // No check constraint here, unlike document_version.status: new audit actions should not need
        // a migration. user_id gets its foreign key with the Identity tables (S1 step 8).
        builder.Property(e => e.Action).HasConversion(new SnakeCaseEnumConverter<AuditAction>()).HasMaxLength(64);
        builder.Property(e => e.EntityType).HasConversion(new SnakeCaseEnumConverter<AuditEntityType>()).HasMaxLength(64);

        // "History of this document": all entries of one entity, newest first.
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.At });
    }
}
