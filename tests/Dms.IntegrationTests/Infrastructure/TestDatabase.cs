using Dms.Infrastructure.Persistence;

namespace Dms.IntegrationTests.Infrastructure;

/// <summary>
/// A migrated database that belongs to one test. Use with <c>await using</c>; disposing drops it.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly Func<CancellationToken, Task> _drop;

    internal TestDatabase(string connectionString, Func<CancellationToken, Task> drop)
    {
        ConnectionString = connectionString;
        _drop = drop;
    }

    public string ConnectionString { get; }

    /// <summary>
    /// A new context for this database. Use a separate context for arranging, acting and asserting,
    /// so the assertions read from the database rather than from the change tracker.
    /// </summary>
    public DmsDbContext CreateDbContext() => PostgresFixture.CreateDbContext(ConnectionString);

    // Not tied to the test's cancellation token: the database must be dropped even when the test was cancelled.
    public async ValueTask DisposeAsync() => await _drop(CancellationToken.None);
}
