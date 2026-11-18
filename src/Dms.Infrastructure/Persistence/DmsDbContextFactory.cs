using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dms.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> at design time, so migrations can be created without starting the API
/// or its dependency injection setup.
/// </summary>
/// <remarks>
/// Creating a migration does not connect to the database. Commands that do (e.g.
/// <c>dotnet ef database update</c>) take the connection string from the <c>ConnectionStrings__dms</c>
/// environment variable; it is never stored in the repository.
/// </remarks>
public sealed class DmsDbContextFactory : IDesignTimeDbContextFactory<DmsDbContext>
{
    private const string PlaceholderConnectionString = "Host=localhost;Database=dms";

    public DmsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__dms") ?? PlaceholderConnectionString;

        var options = new DbContextOptionsBuilder<DmsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DmsDbContext(options);
    }
}
