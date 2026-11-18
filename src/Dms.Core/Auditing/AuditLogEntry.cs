namespace Dms.Core.Auditing;

/// <summary>One row of the DMS audit log: who did what to which entity, and when.</summary>
public sealed class AuditLogEntry
{
    // Used by EF Core to materialise rows; constructor parameters bind to properties by name.
    private AuditLogEntry(Guid id, Guid? userId, AuditAction action, AuditEntityType entityType, Guid entityId,
        DateTimeOffset at)
    {
        Id = id;
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        At = at;
    }

    public Guid Id { get; private set; }

    /// <summary>The acting user; <c>null</c> for actions taken by the system (e.g. activation after processing).</summary>
    public Guid? UserId { get; private set; }

    public AuditAction Action { get; private set; }

    public AuditEntityType EntityType { get; private set; }

    public Guid EntityId { get; private set; }

    public DateTimeOffset At { get; private set; }

    public static AuditLogEntry Create(Guid? userId, AuditAction action, AuditEntityType entityType, Guid entityId,
        DateTimeOffset now)
    {
        now = Guard.Utc(now);
        return new AuditLogEntry(Guid.CreateVersion7(now), userId, action, entityType, Guard.NotEmpty(entityId), now);
    }
}
