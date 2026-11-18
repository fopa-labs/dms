using System.Text.RegularExpressions;

namespace Dms.Core.Documents;

/// <summary>
/// One uploaded file of a <see cref="Document"/>. Created through <see cref="Document.AddVersion"/>.
/// </summary>
/// <remarks>
/// Processing fields (<c>outline</c>, <c>run_id</c>, <c>pending_jobs</c>) and status transitions arrive
/// with the job queue in M2.
/// </remarks>
public sealed partial class DocumentVersion
{
    public const int FileNameMaxLength = 255;
    public const int MimeTypeMaxLength = 127;

    // Used by EF Core to materialise rows; constructor parameters bind to properties by name.
    private DocumentVersion(Guid id, Guid documentId, int versionNo, string blobSha256, string fileName,
        string mimeType, long sizeBytes, DocumentVersionStatus status, DateTimeOffset uploadedAt)
    {
        Id = id;
        DocumentId = documentId;
        VersionNo = versionNo;
        BlobSha256 = blobSha256;
        FileName = fileName;
        MimeType = mimeType;
        SizeBytes = sizeBytes;
        Status = status;
        UploadedAt = uploadedAt;
    }

    public Guid Id { get; private set; }

    public Guid DocumentId { get; private set; }

    /// <summary>1, 2, 3, … per document.</summary>
    public int VersionNo { get; private set; }

    /// <summary>Lowercase hex SHA-256 of the file; also its key in the content-addressed blob store.</summary>
    public string BlobSha256 { get; private set; }

    /// <summary>Original file name, for display and download only; never used as a path.</summary>
    public string FileName { get; private set; }

    /// <summary>Detected from the content, not from the file extension.</summary>
    public string MimeType { get; private set; }

    public long SizeBytes { get; private set; }

    /// <summary>Known after the file has been opened by processing.</summary>
    public int? PageCount { get; private set; }

    public DocumentVersionStatus Status { get; private set; }

    /// <summary>Readable reason when <see cref="Status"/> is <see cref="DocumentVersionStatus.Failed"/>.</summary>
    public string? Error { get; private set; }

    public DateTimeOffset UploadedAt { get; private set; }

    internal static DocumentVersion Create(Guid documentId, int versionNo, string blobSha256, string fileName,
        string mimeType, long sizeBytes, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(versionNo);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeBytes);
        if (!Sha256Pattern().IsMatch(blobSha256))
        {
            throw new ArgumentException("The hash must be 64 lowercase hexadecimal characters.", nameof(blobSha256));
        }

        return new DocumentVersion(Guid.CreateVersion7(now), documentId, versionNo, blobSha256,
            Guard.Text(fileName, FileNameMaxLength), Guard.Text(mimeType, MimeTypeMaxLength), sizeBytes,
            DocumentVersionStatus.Queued, now);
    }

    [GeneratedRegex(@"^[0-9a-f]{64}\z", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Pattern();
}
