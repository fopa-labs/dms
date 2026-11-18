using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dms.Core;

/// <summary>Argument checks shared by the domain entities. Each throws an <see cref="ArgumentException"/>.</summary>
internal static partial class Guard
{
    /// <summary>
    /// Timestamps are stored as PostgreSQL <c>timestamptz</c>; Npgsql only writes a
    /// <see cref="DateTimeOffset"/> with offset zero, so a non-UTC value is rejected here, at the source.
    /// </summary>
    public static DateTimeOffset Utc(DateTimeOffset value, [CallerArgumentExpression(nameof(value))] string? paramName = null) =>
        value.Offset == TimeSpan.Zero
            ? value
            : throw new ArgumentException("The timestamp must be in UTC (offset zero).", paramName);

    public static Guid NotEmpty(Guid value, [CallerArgumentExpression(nameof(value))] string? paramName = null) =>
        value != Guid.Empty ? value : throw new ArgumentException("The ID must not be empty.", paramName);

    /// <summary>Trims the value and checks that it is not empty and not longer than <paramref name="maxLength"/>.</summary>
    public static string Text(string value, int maxLength, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength
            ? trimmed
            : throw new ArgumentException($"The value must be at most {maxLength} characters long.", paramName);
    }

    /// <summary>
    /// Lowercase ASCII letters and digits in hyphen-separated groups, e.g. <c>demo-manuals</c>.
    /// Used for collection slugs and domain pack IDs, which appear in URLs and configuration.
    /// </summary>
    public static string Slug(string value, int maxLength, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value.Length <= maxLength && SlugPattern().IsMatch(value)
            ? value
            : throw new ArgumentException(
                $"The value must be lowercase letters and digits separated by single hyphens, at most {maxLength} characters.",
                paramName);
    }

    /// <summary>Lowercase ISO 639-1 language code, e.g. <c>en</c> or <c>hu</c>.</summary>
    public static string LanguageCode(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return LanguagePattern().IsMatch(value)
            ? value
            : throw new ArgumentException("The language must be a lowercase ISO 639-1 code, e.g. 'en'.", paramName);
    }

    // \z instead of $: $ also matches before a trailing newline.
    [GeneratedRegex(@"^[a-z0-9]+(-[a-z0-9]+)*\z", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();

    [GeneratedRegex(@"^[a-z]{2}\z", RegexOptions.CultureInvariant)]
    private static partial Regex LanguagePattern();
}
