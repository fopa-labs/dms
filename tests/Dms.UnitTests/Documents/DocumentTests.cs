using Dms.Core.Documents;

namespace Dms.UnitTests.Documents;

public class DocumentTests
{
    private const string Sha = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08";
    private static readonly DateTimeOffset _now = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid _collectionId = Guid.CreateVersion7();

    [Fact]
    public void Create_sets_fields_and_timestamps()
    {
        var document = Document.Create(_collectionId, "Owner's manual", _now, lang: "en");

        Assert.Equal(7, document.Id.Version);
        Assert.Equal(_collectionId, document.CollectionId);
        Assert.Equal("Owner's manual", document.Title);
        Assert.Equal("en", document.Lang);
        Assert.Null(document.ActiveVersionId);
        Assert.Empty(document.Metadata);
        Assert.Empty(document.Versions);
        Assert.Empty(document.Tags);
        Assert.Equal(_now, document.CreatedAt);
        Assert.Equal(_now, document.UpdatedAt);
        Assert.Null(document.DeletedAt);
        Assert.False(document.IsDeleted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_rejects_an_empty_title(string title) =>
        Assert.ThrowsAny<ArgumentException>(() => Document.Create(_collectionId, title, _now));

    [Fact]
    public void Create_rejects_an_invalid_language() =>
        Assert.ThrowsAny<ArgumentException>(() => Document.Create(_collectionId, "Title", _now, lang: "english"));

    [Fact]
    public void Create_rejects_an_empty_collection_id() =>
        Assert.ThrowsAny<ArgumentException>(() => Document.Create(Guid.Empty, "Title", _now));

    [Fact]
    public void AddVersion_numbers_versions_from_one_and_starts_them_queued()
    {
        var document = Document.Create(_collectionId, "Manual", _now);
        var later = _now.AddMinutes(5);

        var first = document.AddVersion(Sha, "manual.pdf", "application/pdf", 1024, _now);
        var second = document.AddVersion(Sha.Replace('9', '8'), "manual-v2.pdf", "application/pdf", 2048, later);

        Assert.Equal(1, first.VersionNo);
        Assert.Equal(2, second.VersionNo);
        Assert.Equal(DocumentVersionStatus.Queued, second.Status);
        Assert.Equal(document.Id, second.DocumentId);
        Assert.Equal(7, second.Id.Version);
        Assert.Equal([first, second], document.Versions);
        Assert.Equal(later, document.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08")]
    [InlineData("9f86d081")]
    public void AddVersion_rejects_a_hash_that_is_not_lowercase_hex_sha256(string sha) =>
        Assert.ThrowsAny<ArgumentException>(() =>
            Document.Create(_collectionId, "Manual", _now).AddVersion(sha, "a.pdf", "application/pdf", 1, _now));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddVersion_rejects_a_non_positive_size(long size) =>
        Assert.ThrowsAny<ArgumentException>(() =>
            Document.Create(_collectionId, "Manual", _now).AddVersion(Sha, "a.pdf", "application/pdf", size, _now));

    [Fact]
    public void SoftDelete_marks_the_document_deleted_once()
    {
        var document = Document.Create(_collectionId, "Manual", _now);
        var deletedAt = _now.AddDays(1);

        document.SoftDelete(deletedAt);
        document.SoftDelete(deletedAt.AddDays(1));

        Assert.True(document.IsDeleted);
        Assert.Equal(deletedAt, document.DeletedAt);
        Assert.Equal(deletedAt, document.UpdatedAt);
    }

    [Fact]
    public void AddTag_ignores_a_tag_that_is_already_attached()
    {
        var document = Document.Create(_collectionId, "Manual", _now);
        var tag = Tag.Create("engine");

        document.AddTag(tag, _now);
        document.AddTag(tag, _now);

        Assert.Single(document.Tags);
    }

    [Fact]
    public void RemoveTag_detaches_the_tag()
    {
        var document = Document.Create(_collectionId, "Manual", _now);
        var tag = Tag.Create("engine");
        document.AddTag(tag, _now);

        document.RemoveTag(tag, _now);

        Assert.Empty(document.Tags);
    }
}
