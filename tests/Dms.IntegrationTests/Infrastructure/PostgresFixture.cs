using Dms.Infrastructure.Persistence;
using Dms.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

// One PostgreSQL container for the whole test assembly (xUnit v3 assembly fixture): starting a
// container takes seconds, creating a database from a template takes milliseconds.
[assembly: AssemblyFixture(typeof(PostgresFixture))]

namespace Dms.IntegrationTests.Infrastructure;

/// <summary>
/// Starts PostgreSQL with pgvector in Docker, applies the EF Core migrations once to a template
/// database, and hands out a fresh copy of it per test (<see cref="CreateDatabaseAsync"/>).
/// </summary>
/// <remarks>
/// Isolation by database instead of by cleanup: every test gets its own database created with
/// <c>CREATE DATABASE … TEMPLATE</c>, so tests can run in parallel, need no ordering and leave nothing
/// behind for the next one. Inject it with a constructor parameter of this type.
/// </remarks>
public sealed class PostgresFixture : IAsyncLifetime
{
    /// <summary>Same image as the AppHost (src/Dms.AppHost/AppHost.cs); keep the two in sync.</summary>
    public const string Image = "pgvector/pgvector:0.8.7-pg17";

    private const string TemplateDatabase = "dms_template";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image)
        .WithDatabase(TemplateDatabase)
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var templateConnectionString = _container.GetConnectionString();
        await using (var db = CreateDbContext(templateConnectionString))
        {
            await db.Database.MigrateAsync();
        }

        // PostgreSQL refuses to copy a template that has open connections; Npgsql keeps pooled
        // connections open after the context is disposed, so the pool is cleared explicitly.
        await using var connection = new NpgsqlConnection(templateConnectionString);
        NpgsqlConnection.ClearPool(connection);
    }

    /// <summary>Creates an empty, fully migrated database for one test. Dispose it to drop it.</summary>
    public async Task<TestDatabase> CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        // Generated, not user input, so it is safe to put into the SQL text (identifiers cannot be parameters).
        var name = $"test_{Guid.CreateVersion7():N}";

        await ExecuteOnServerAsync($"""CREATE DATABASE "{name}" TEMPLATE "{TemplateDatabase}" """, cancellationToken);

        var connectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = name }
            .ConnectionString;

        return new TestDatabase(connectionString, ct => DropDatabaseAsync(name, connectionString, ct));
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();

    internal static DmsDbContext CreateDbContext(string connectionString) =>
        new(new DbContextOptionsBuilder<DmsDbContext>().UseNpgsql(connectionString).Options);

    private async Task DropDatabaseAsync(string name, string connectionString, CancellationToken cancellationToken)
    {
        await using (var connection = new NpgsqlConnection(connectionString))
        {
            NpgsqlConnection.ClearPool(connection);
        }

        // FORCE terminates connections a test may have left open.
        await ExecuteOnServerAsync($"""DROP DATABASE IF EXISTS "{name}" WITH (FORCE)""", cancellationToken);
    }

    /// <summary>Runs a statement on the maintenance database; CREATE/DROP DATABASE cannot run inside the target.</summary>
    private async Task ExecuteOnServerAsync(string sql, CancellationToken cancellationToken)
    {
        var connectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = "postgres" }
            .ConnectionString;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
