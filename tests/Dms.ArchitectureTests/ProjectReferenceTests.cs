using System.Xml.Linq;

namespace Dms.ArchitectureTests;

/// <summary>
/// Checks the project references declared in the .csproj files against the dependency rules in
/// CLAUDE.md. This catches a forbidden reference as soon as it is added, even before any type
/// uses it (the compiler drops unused references from the assembly metadata, so type-level
/// checks alone would miss it).
/// </summary>
public class ProjectReferenceTests
{
    private const string Core = "Dms.Core";
    private const string Infrastructure = "Dms.Infrastructure";

    // Every project under src/ and the exact set of solution projects it may reference.
    // Domain packs are matched by the "Dms.Packs." prefix (see AllowedReferences).
    private static readonly Dictionary<string, string[]> _allowed = new()
    {
        [Core] = [],
        [Infrastructure] = [Core],
        ["Dms.Api"] = [Core, Infrastructure],
        ["Dms.Worker"] = [Core, Infrastructure],
    };

    public static TheoryData<string> SourceProjects() =>
        [.. SolutionLayout.SourceProjectFiles().Select(Path.GetFileNameWithoutExtension).OfType<string>()];

    [Theory]
    [MemberData(nameof(SourceProjects))]
    public void Project_references_follow_the_dependency_rules(string project)
    {
        var allowed = AllowedReferences(project);
        var actual = ProjectReferences(project);

        var forbidden = actual.Except(allowed).ToArray();

        Assert.True(
            forbidden.Length == 0,
            $"{project} must not reference {string.Join(", ", forbidden)}. Allowed: [{string.Join(", ", allowed)}].");
    }

    [Fact]
    public void Every_source_project_has_a_rule()
    {
        // A new project under src/ must get an explicit rule here instead of passing silently.
        var unknown = SolutionLayout.SourceProjectFiles()
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(p => !_allowed.ContainsKey(p) && !IsDomainPack(p))
            .ToArray();

        Assert.Empty(unknown);
    }

    private static string[] AllowedReferences(string project) =>
        IsDomainPack(project) ? [Core] : _allowed.GetValueOrDefault(project, []);

    private static bool IsDomainPack(string project) => project.StartsWith("Dms.Packs.", StringComparison.Ordinal);

    private static string[] ProjectReferences(string project)
    {
        var file = SolutionLayout.SourceProjectFiles()
            .Single(f => Path.GetFileNameWithoutExtension(f) == project);

        return [.. XDocument.Load(file)
            .Descendants("ProjectReference")
            .Select(r => (string?)r.Attribute("Include"))
            .OfType<string>()
            // Include paths use backslashes on every OS; normalise before taking the file name.
            .Select(include => Path.GetFileNameWithoutExtension(include.Replace('\\', '/')))];
    }
}
