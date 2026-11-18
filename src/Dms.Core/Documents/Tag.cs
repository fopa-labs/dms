namespace Dms.Core.Documents;

/// <summary>A free-form label; names are unique after normalisation.</summary>
public sealed class Tag
{
    public const int NameMaxLength = 64;

    // Used by EF Core to materialise rows; constructor parameters bind to properties by name.
    private Tag(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }

    /// <summary>Trimmed and lowercased, so "Engine" and "engine " are the same tag.</summary>
    public string Name { get; private set; }

    public static Tag Create(string name) =>
        new(Guid.CreateVersion7(), Guard.Text(name, NameMaxLength).ToLowerInvariant());
}
