using System.Collections.Frozen;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dms.Infrastructure.Persistence.Conversions;

/// <summary>
/// Stores an enum as snake_case text (<c>DocumentUploaded</c> → <c>document_uploaded</c>), so rows stay
/// readable in SQL and in the spec's terms, and reordering enum members cannot corrupt data.
/// </summary>
public sealed class SnakeCaseEnumConverter<TEnum>()
    : ValueConverter<TEnum, string>(value => ToStored(value), stored => FromStored(stored))
    where TEnum : struct, Enum
{
    private static readonly FrozenDictionary<TEnum, string> _toStored =
        Enum.GetValues<TEnum>().ToFrozenDictionary(v => v, v => JsonNamingPolicy.SnakeCaseLower.ConvertName(v.ToString()));

    private static readonly FrozenDictionary<string, TEnum> _fromStored =
        _toStored.ToFrozenDictionary(kv => kv.Value, kv => kv.Key, StringComparer.Ordinal);

    /// <summary>Every stored value in declaration order; used for check constraints.</summary>
    public IReadOnlyList<string> StoredValues { get; } = [.. Enum.GetValues<TEnum>().Select(v => _toStored[v])];

    private static string ToStored(TEnum value) => _toStored[value];

    // An unknown value means the database holds something the code does not know: fail instead of guessing.
    private static TEnum FromStored(string stored) =>
        _fromStored.TryGetValue(stored, out var value)
            ? value
            : throw new InvalidOperationException($"'{stored}' is not a stored value of {typeof(TEnum).Name}.");
}
