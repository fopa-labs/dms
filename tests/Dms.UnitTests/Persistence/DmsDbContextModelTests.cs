using Dms.Core.Auditing;
using Dms.Core.Documents;
using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Dms.UnitTests.Persistence;

/// <summary>
/// Checks the EF Core model without a database: the provider is configured, but no connection is opened.
/// Applying the migrations to a real PostgreSQL is covered by the integration tests.
/// </summary>
public sealed class DmsDbContextModelTests : IDisposable
{
    private readonly DmsDbContext _context = new DmsDbContextFactory().CreateDbContext([]);

    public void Dispose() => _context.Dispose();

    [Fact]
    public void Model_has_no_changes_missing_from_the_migrations() =>
        Assert.False(_context.Database.HasPendingModelChanges(),
            "The model differs from the last migration; add a migration (see CLAUDE.md, Commands).");

    [Theory]
    [InlineData(typeof(Collection), "collection")]
    [InlineData(typeof(Document), "document")]
    [InlineData(typeof(DocumentVersion), "document_version")]
    [InlineData(typeof(Tag), "tag")]
    [InlineData(typeof(AuditLogEntry), "audit_log")]
    public void Entities_map_to_the_tables_in_the_spec(Type entity, string table) =>
        Assert.Equal(table, Entity(entity).GetTableName());

    [Fact]
    public void Document_tags_use_the_document_tag_join_table()
    {
        var join = Assert.Single(_context.Model.GetEntityTypes(), e => e.GetTableName() == "document_tag");

        Assert.Equal(["document_id", "tag_id"], join.FindPrimaryKey()!.Properties.Select(Column));
    }

    [Fact]
    public void Columns_are_snake_case() =>
        Assert.Equal("blob_sha256", Column(Entity(typeof(DocumentVersion)).FindProperty(nameof(DocumentVersion.BlobSha256))!));

    [Fact]
    public void Ids_are_generated_by_the_application_not_the_database() =>
        Assert.All(
            [typeof(Collection), typeof(Document), typeof(DocumentVersion), typeof(Tag), typeof(AuditLogEntry)],
            type => Assert.Equal(ValueGenerated.Never, Entity(type).FindProperty("Id")!.ValueGenerated));

    [Fact]
    public void Document_uses_xmin_as_concurrency_token()
    {
        var token = Assert.Single(Entity(typeof(Document)).GetProperties(), p => p.IsConcurrencyToken);

        Assert.Equal("xmin", Column(token));
    }

    [Theory]
    [InlineData(typeof(Collection), nameof(Collection.Slug))]
    [InlineData(typeof(Tag), nameof(Tag.Name))]
    public void Natural_keys_are_unique(Type entity, string property) =>
        Assert.Contains(Entity(entity).GetIndexes(),
            i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual([property]));

    [Fact]
    public void Version_numbers_are_unique_per_document() =>
        Assert.Contains(Entity(typeof(DocumentVersion)).GetIndexes(),
            i => i.IsUnique && i.Properties.Select(p => p.Name)
                .SequenceEqual([nameof(DocumentVersion.DocumentId), nameof(DocumentVersion.VersionNo)]));

    // Extensions are design-time (migration) annotations; the runtime model drops them.
    [Fact]
    public void Extensions_for_later_milestones_are_installed() =>
        Assert.Equal(["pg_trgm", "vector"],
            _context.GetService<IDesignTimeModel>().Model.GetPostgresExtensions().Select(e => e.Name).Order());

    private IEntityType Entity(Type type) => _context.Model.FindEntityType(type)!;

    private static string Column(IProperty property) =>
        property.GetColumnName(StoreObjectIdentifier.Table(property.DeclaringType.GetTableName()!))!;
}
