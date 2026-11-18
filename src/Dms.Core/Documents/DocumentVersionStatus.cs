namespace Dms.Core.Documents;

/// <summary>Processing state of a <see cref="DocumentVersion"/>, as shown in the DMS.</summary>
public enum DocumentVersionStatus
{
    Queued,
    Processing,
    Indexed,
    Failed,
}
