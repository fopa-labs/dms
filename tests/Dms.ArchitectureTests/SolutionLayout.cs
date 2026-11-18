namespace Dms.ArchitectureTests;

/// <summary>Locates the repository and its source projects from the test output directory.</summary>
internal static class SolutionLayout
{
    private static readonly Lazy<string> _root = new(FindRoot);

    public static string RepositoryRoot => _root.Value;

    /// <summary>All .csproj files under src/, including domain packs under src/Packs/.</summary>
    public static IEnumerable<string> SourceProjectFiles() =>
        Directory.EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(f => !IsBuildOutput(f));

    private static bool IsBuildOutput(string path) =>
        path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj");

    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Dms.slnx")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException($"Dms.slnx not found above {AppContext.BaseDirectory}.");
    }
}
