namespace Dms.Core.Auditing;

/// <summary>What happened. Stored as text, so new values can be added without a migration.</summary>
public enum AuditAction
{
    CollectionCreated,
    DocumentUploaded,
    DocumentUpdated,
    DocumentDeleted,
    VersionUploaded,
    VersionActivated,
}

/// <summary>The kind of entity an <see cref="AuditLogEntry"/> refers to.</summary>
public enum AuditEntityType
{
    Collection,
    Document,
    DocumentVersion,
}
