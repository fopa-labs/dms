using Dms.Core.Auditing;

namespace Dms.UnitTests.Auditing;

public class AuditLogEntryTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_records_who_did_what_to_which_entity_and_when()
    {
        var userId = Guid.CreateVersion7();
        var documentId = Guid.CreateVersion7();

        var entry = AuditLogEntry.Create(userId, AuditAction.DocumentUploaded, AuditEntityType.Document, documentId, _now);

        Assert.Equal(7, entry.Id.Version);
        Assert.Equal(userId, entry.UserId);
        Assert.Equal(AuditAction.DocumentUploaded, entry.Action);
        Assert.Equal(AuditEntityType.Document, entry.EntityType);
        Assert.Equal(documentId, entry.EntityId);
        Assert.Equal(_now, entry.At);
    }

    [Fact]
    public void Create_allows_a_system_action_without_a_user()
    {
        var entry = AuditLogEntry.Create(null, AuditAction.VersionActivated, AuditEntityType.DocumentVersion,
            Guid.CreateVersion7(), _now);

        Assert.Null(entry.UserId);
    }

    [Fact]
    public void Create_rejects_an_empty_entity_id() =>
        Assert.ThrowsAny<ArgumentException>(() =>
            AuditLogEntry.Create(null, AuditAction.DocumentDeleted, AuditEntityType.Document, Guid.Empty, _now));
}
