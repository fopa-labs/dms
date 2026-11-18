using Dms.Core.Documents;

namespace Dms.UnitTests.Documents;

public class CollectionTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_sets_fields_and_a_version_7_id()
    {
        var collection = Collection.Create("demo-manuals", "Demo manuals", "en", "english", _now,
            description: "Freely licensed manuals");

        Assert.Equal(7, collection.Id.Version);
        Assert.Equal("demo-manuals", collection.Slug);
        Assert.Equal("Demo manuals", collection.Name);
        Assert.Equal("Freely licensed manuals", collection.Description);
        Assert.Equal("en", collection.Lang);
        Assert.Equal("english", collection.TsConfig);
        Assert.Null(collection.DomainPack);
        Assert.Empty(collection.Settings);
        Assert.Equal(_now, collection.CreatedAt);
    }

    [Fact]
    public void Create_accepts_a_domain_pack()
    {
        var collection = Collection.Create("repair", "Repair manual", "en", "english", _now, domainPack: "automotive");

        Assert.Equal("automotive", collection.DomainPack);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Demo")]
    [InlineData("demo manuals")]
    [InlineData("-demo")]
    [InlineData("demo-")]
    [InlineData("demo--manuals")]
    [InlineData("démo")]
    public void Create_rejects_an_invalid_slug(string slug) =>
        Assert.ThrowsAny<ArgumentException>(() => Collection.Create(slug, "Name", "en", "english", _now));

    [Fact]
    public void Create_rejects_a_slug_longer_than_the_limit() =>
        Assert.ThrowsAny<ArgumentException>(() =>
            Collection.Create(new string('a', Collection.SlugMaxLength + 1), "Name", "en", "english", _now));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_an_empty_name(string name) =>
        Assert.ThrowsAny<ArgumentException>(() => Collection.Create("demo", name, "en", "english", _now));

    [Theory]
    [InlineData("")]
    [InlineData("eng")]
    [InlineData("EN")]
    public void Create_rejects_a_language_that_is_not_a_two_letter_code(string lang) =>
        Assert.ThrowsAny<ArgumentException>(() => Collection.Create("demo", "Name", lang, "english", _now));

    [Theory]
    [InlineData("")]
    [InlineData("English")]
    [InlineData("english; drop table x")]
    public void Create_rejects_an_invalid_text_search_configuration(string tsConfig) =>
        Assert.ThrowsAny<ArgumentException>(() => Collection.Create("demo", "Name", "en", tsConfig, _now));

    [Theory]
    [InlineData("")]
    [InlineData("Automotive")]
    public void Create_rejects_an_invalid_domain_pack_id(string domainPack) =>
        Assert.ThrowsAny<ArgumentException>(() =>
            Collection.Create("demo", "Name", "en", "english", _now, domainPack: domainPack));

    [Fact]
    public void Create_rejects_a_non_utc_timestamp() =>
        Assert.ThrowsAny<ArgumentException>(() =>
            Collection.Create("demo", "Name", "en", "english", new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.FromHours(1))));
}
