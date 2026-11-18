using Dms.Core.Documents;

namespace Dms.UnitTests.Documents;

public class TagTests
{
    [Theory]
    [InlineData("engine", "engine")]
    [InlineData("  Engine ", "engine")]
    [InlineData("Wiring Diagrams", "wiring diagrams")]
    public void Create_normalises_the_name(string input, string expected) =>
        Assert.Equal(expected, Tag.Create(input).Name);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_an_empty_name(string name) =>
        Assert.ThrowsAny<ArgumentException>(() => Tag.Create(name));

    [Fact]
    public void Create_rejects_a_name_longer_than_the_limit() =>
        Assert.ThrowsAny<ArgumentException>(() => Tag.Create(new string('a', Tag.NameMaxLength + 1)));
}
