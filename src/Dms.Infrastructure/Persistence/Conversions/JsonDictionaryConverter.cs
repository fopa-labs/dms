using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dms.Infrastructure.Persistence.Conversions;

/// <summary>
/// Maps a string-to-string dictionary to a <c>jsonb</c> column. Npgsql would map
/// <see cref="Dictionary{TKey,TValue}"/> to <c>hstore</c> by default; jsonb is what the spec uses and
/// what the GIN index and JSON operators work on.
/// </summary>
public sealed class JsonDictionaryConverter()
    : ValueConverter<Dictionary<string, string>, string>(value => Serialize(value), json => Deserialize(json))
{
    private static string Serialize(Dictionary<string, string> value) => JsonSerializer.Serialize(value);

    private static Dictionary<string, string> Deserialize(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
}

/// <summary>
/// Change tracking for a mutable dictionary: EF Core keeps a copy (snapshot) and compares contents,
/// so <c>metadata["key"] = "value"</c> is detected and saved. Without it only replacing the whole
/// dictionary would be noticed.
/// </summary>
public sealed class JsonDictionaryComparer()
    : ValueComparer<Dictionary<string, string>>(
        (a, b) => AreEqual(a, b),
        value => HashCode(value),
        value => new Dictionary<string, string>(value))
{
    private static bool AreEqual(Dictionary<string, string>? a, Dictionary<string, string>? b) =>
        ReferenceEquals(a, b)
        || (a is not null && b is not null && a.Count == b.Count
            && a.All(kv => b.TryGetValue(kv.Key, out var other) && other == kv.Value));

    // Order-independent, so two dictionaries with the same entries hash the same.
    private static int HashCode(Dictionary<string, string> value) =>
        value.Aggregate(0, (hash, kv) => hash ^ System.HashCode.Combine(kv.Key, kv.Value));
}
