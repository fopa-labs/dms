namespace Dms.Core.Documents;

/// <summary>
/// A DMS document. Its content lives in versions; only the active version is searchable.
/// </summary>
public sealed class Document
{
    public const int TitleMaxLength = 500;

    private readonly List<DocumentVersion> _versions = [];
    private readonly List<Tag> _tags = [];

    // Used by EF Core to materialise rows; constructor parameters bind to properties by name.
    private Document(Guid id, Guid collectionId, string title, string? lang, DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        CollectionId = collectionId;
        Title = title;
        Lang = lang;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }

    public Guid CollectionId { get; private set; }

    public string Title { get; private set; }

    /// <summary>
    /// The version search uses. Set by activation (processing finished, or rollback), which happens
    /// in SQL together with the chunks' <c>active</c> flags, so there is no setter here.
    /// </summary>
    public Guid? ActiveVersionId { get; private set; }

    /// <summary>Free key-value metadata (stored as <c>jsonb</c>, filterable).</summary>
    public Dictionary<string, string> Metadata { get; private set; } = [];

    /// <summary>ISO 639-1 language code, detected automatically later (M8); <c>null</c> when unknown.</summary>
    public string? Lang { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Last change; together with <see cref="Id"/> the key of cursor pagination.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt is not null;

    public IReadOnlyList<DocumentVersion> Versions => _versions;

    public IReadOnlyList<Tag> Tags => _tags;

    public static Document Create(Guid collectionId, string title, DateTimeOffset now, string? lang = null)
    {
        now = Guard.Utc(now);

        return new Document(Guid.CreateVersion7(now), Guard.NotEmpty(collectionId), Guard.Text(title, TitleMaxLength),
            lang is null ? null : Guard.LanguageCode(lang), now, now);
    }

    /// <summary>
    /// Adds the next version, numbered after the highest loaded one. The versions must be loaded;
    /// two concurrent uploads that compute the same number are stopped by the unique
    /// <c>(document_id, version_no)</c> index.
    /// </summary>
    public DocumentVersion AddVersion(string blobSha256, string fileName, string mimeType, long sizeBytes,
        DateTimeOffset now)
    {
        now = Guard.Utc(now);
        var versionNo = _versions.Count == 0 ? 1 : _versions.Max(v => v.VersionNo) + 1;

        var version = DocumentVersion.Create(Id, versionNo, blobSha256, fileName, mimeType, sizeBytes, now);
        _versions.Add(version);
        UpdatedAt = now;
        return version;
    }

    /// <summary>Soft delete; repeated calls keep the first deletion time.</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAt = UpdatedAt = Guard.Utc(now);
    }

    public void AddTag(Tag tag, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (_tags.Any(t => t.Id == tag.Id))
        {
            return;
        }

        _tags.Add(tag);
        UpdatedAt = Guard.Utc(now);
    }

    public void RemoveTag(Tag tag, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (_tags.RemoveAll(t => t.Id == tag.Id) > 0)
        {
            UpdatedAt = Guard.Utc(now);
        }
    }
}
