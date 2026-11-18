using Dms.Core.Documents;
using Microsoft.EntityFrameworkCore;

namespace Dms.IntegrationTests.Infrastructure;

/// <summary>Checks the fixture itself: real migrations on real PostgreSQL, and isolation between tests.</summary>
public class PostgresFixtureTests(PostgresFixture postgres)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Test_database_has_every_migration_applied()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await using var db = database.CreateDbContext();

        var applied = await db.Database.GetAppliedMigrationsAsync(Ct);

        Assert.Equal(db.Database.GetMigrations(), applied);
    }

    [Fact]
    public async Task Extensions_are_installed()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await using var db = database.CreateDbContext();

        var extensions = await db.Database
            .SqlQuery<string>($"SELECT extname AS \"Value\" FROM pg_extension WHERE extname IN ('vector', 'pg_trgm') ORDER BY extname")
            .ToListAsync(Ct);

        Assert.Equal(["pg_trgm", "vector"], extensions);
    }

    [Fact]
    public async Task Each_test_database_is_isolated()
    {
        await using var first = await postgres.CreateDatabaseAsync(Ct);
        await using var second = await postgres.CreateDatabaseAsync(Ct);

        await using (var db = first.CreateDbContext())
        {
            db.Tags.Add(Tag.Create("engine"));
            await db.SaveChangesAsync(Ct);
        }

        await using var other = second.CreateDbContext();
        Assert.Equal(0, await other.Tags.CountAsync(Ct));
    }
}
