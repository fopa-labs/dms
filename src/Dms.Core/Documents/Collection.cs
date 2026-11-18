using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Dms.Core.Documents;

/// <summary>
/// A set of documents searched together; the scope of search and of the agent.
/// </summary>
// CA1711 reserves the "Collection" suffix for collection types. Here it is the domain term from the
// glossary and the name of the table; any rename that keeps the meaning still ends in "Collection".
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Domain term (glossary: Collection), not a .NET collection type.")]
public sealed partial class Collection
{
    public const int SlugMaxLength = 64;
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int TsConfigMaxLength = 63;
    public const int DomainPackMaxLength = 64;

    // Used by EF Core to materialise rows; constructor parameters bind to properties by name.
    private Collection(Guid id, string slug, string name, string? description, string lang, string tsConfig,
        string? domainPack, DateTimeOffset createdAt)
    {
        Id = id;
        Slug = slug;
        Name = name;
        Description = description;
        Lang = lang;
        TsConfig = tsConfig;
        DomainPack = domainPack;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    /// <summary>URL-safe unique key, e.g. <c>demo-manuals</c>.</summary>
    public string Slug { get; private set; }

    public string Name { get; private set; }

    /// <summary>Shown to the agent by <c>list_collections</c>, so it should say what the collection contains.</summary>
    public string? Description { get; private set; }

    /// <summary>ISO 639-1 language code of the content, e.g. <c>en</c> or <c>hu</c>.</summary>
    public string Lang { get; private set; }

    /// <summary>PostgreSQL text search configuration for the full-text column, e.g. <c>english</c>.</summary>
    public string TsConfig { get; private set; }

    /// <summary>ID of the domain pack this collection uses, or <c>null</c> for the general behaviour.</summary>
    public string? DomainPack { get; private set; }

    /// <summary>Collection-level overrides of pack or default settings (stored as <c>jsonb</c>).</summary>
    public Dictionary<string, string> Settings { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public static Collection Create(string slug, string name, string lang, string tsConfig, DateTimeOffset now,
        string? description = null, string? domainPack = null)
    {
        now = Guard.Utc(now);

        return new Collection(
            Guid.CreateVersion7(now),
            Guard.Slug(slug, SlugMaxLength),
            Guard.Text(name, NameMaxLength),
            description is null ? null : Guard.Text(description, DescriptionMaxLength),
            Guard.LanguageCode(lang),
            TextSearchConfig(tsConfig),
            domainPack is null ? null : Guard.Slug(domainPack, DomainPackMaxLength),
            now);
    }

    /// <summary>
    /// The value ends up as a <c>regconfig</c> in generated columns and queries, so only a plain
    /// lowercase identifier is accepted; whether the configuration exists is checked by PostgreSQL.
    /// </summary>
    private static string TextSearchConfig(string tsConfig) =>
        tsConfig.Length <= TsConfigMaxLength && IdentifierPattern().IsMatch(tsConfig)
            ? tsConfig
            : throw new ArgumentException("The text search configuration must be a lowercase identifier, e.g. 'english'.",
                nameof(tsConfig));

    [GeneratedRegex(@"^[a-z_][a-z0-9_]*\z", RegexOptions.CultureInvariant)]
    private static partial Regex IdentifierPattern();
}
