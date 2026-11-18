using Dms.Core.Auditing;
using Dms.Core.Documents;
using Dms.Infrastructure.Persistence.Conversions;

namespace Dms.UnitTests.Persistence;

public class ConverterTests
{
    [Theory]
    [InlineData(DocumentVersionStatus.Queued, "queued")]
    [InlineData(DocumentVersionStatus.Indexed, "indexed")]
    public void Enum_is_stored_as_snake_case_text(DocumentVersionStatus value, string stored)
    {
        var converter = new SnakeCaseEnumConverter<DocumentVersionStatus>();

        Assert.Equal(stored, converter.ConvertToProvider(value));
        Assert.Equal(value, converter.ConvertFromProvider(stored));
    }

    [Fact]
    public void Multi_word_enum_values_are_separated_by_underscores() =>
        Assert.Equal("document_uploaded", new SnakeCaseEnumConverter<AuditAction>().ConvertToProvider(AuditAction.DocumentUploaded));

    [Fact]
    public void Allowed_values_list_every_enum_member() =>
        Assert.Equal(["queued", "processing", "indexed", "failed"], new SnakeCaseEnumConverter<DocumentVersionStatus>().StoredValues);

    [Fact]
    public void Unknown_stored_value_fails_loudly() =>
        Assert.ThrowsAny<Exception>(() => new SnakeCaseEnumConverter<DocumentVersionStatus>().ConvertFromProvider("archived"));

    [Fact]
    public void Dictionary_round_trips_through_json()
    {
        var converter = new JsonDictionaryConverter();
        var value = new Dictionary<string, string> { ["model"] = "Demo 3", ["year"] = "2019" };

        var json = (string)converter.ConvertToProvider(value)!;
        var back = (Dictionary<string, string>)converter.ConvertFromProvider(json)!;

        Assert.Equal(value, back);
    }

    [Fact]
    public void Dictionary_comparer_detects_a_changed_value()
    {
        var comparer = new JsonDictionaryComparer();
        var original = new Dictionary<string, string> { ["a"] = "1" };
        var snapshot = comparer.Snapshot(original);

        original["a"] = "2";

        Assert.False(comparer.Equals(original, snapshot));
    }
}
