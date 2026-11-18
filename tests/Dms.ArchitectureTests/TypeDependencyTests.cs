using System.Reflection;
using NetArchTest.Rules;

namespace Dms.ArchitectureTests;

/// <summary>
/// Checks what the compiled types actually depend on. Complements <see cref="ProjectReferenceTests"/>:
/// this also covers package dependencies (e.g. EF Core in Core), which project references do not show.
/// </summary>
public class TypeDependencyTests
{
    // Infrastructure packages that must stay out of Core (CLAUDE.md, "Dependency rules").
    private static readonly string[] _infrastructurePackages =
    [
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "Pgvector",
        "Microsoft.ML.OnnxRuntime",
        "Microsoft.AspNetCore",
    ];

    [Fact]
    public void Core_depends_on_no_other_solution_project_or_infrastructure_package() =>
        AssertNoDependency(
            "Dms.Core",
            ["Dms.Infrastructure", "Dms.Api", "Dms.Worker", "Dms.Packs", .. _infrastructurePackages]);

    [Fact]
    public void Infrastructure_depends_on_no_host_or_domain_pack() =>
        AssertNoDependency("Dms.Infrastructure", ["Dms.Api", "Dms.Worker", "Dms.Packs"]);

    [Fact]
    public void Api_does_not_depend_on_Worker() =>
        AssertNoDependency("Dms.Api", ["Dms.Worker"]);

    [Fact]
    public void Worker_does_not_depend_on_Api() =>
        AssertNoDependency("Dms.Worker", ["Dms.Api"]);

    public static TheoryData<string> DomainPacks() => [.. DomainPackNames()];

    [Theory]
    [MemberData(nameof(DomainPacks))]
    public void Domain_pack_depends_only_on_Core(string pack) =>
        AssertNoDependency(
            pack,
            ["Dms.Infrastructure", "Dms.Api", "Dms.Worker", .. DomainPackNames().Where(p => p != pack)]);

    private static void AssertNoDependency(string assemblyName, string[] forbidden)
    {
        var assembly = Assembly.Load(new AssemblyName(assemblyName));

        // HaveDependencyOnAny matches namespace prefixes, e.g. "Npgsql" also covers "Npgsql.Internal".
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"{assemblyName} must not depend on {string.Join(", ", forbidden)}. " +
            $"Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    private static IEnumerable<string> DomainPackNames() =>
        SolutionLayout.SourceProjectFiles()
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(p => p.StartsWith("Dms.Packs.", StringComparison.Ordinal));
}
