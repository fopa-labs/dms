using System.Diagnostics;
using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dms.MigrationService;

/// <summary>
/// Applies pending migrations once and stops the host. A failure sets a non-zero exit code, so the
/// AppHost does not start the services that wait for this one (<c>WaitForCompletion</c>).
/// </summary>
internal sealed partial class MigrationRunner(
    IServiceScopeFactory scopeFactory,
    IHostApplicationLifetime lifetime,
    IHostEnvironment environment,
    ILogger<MigrationRunner> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Same name as the source registered in ServiceDefaults, so the span shows up in the dashboard.
        using var activitySource = new ActivitySource(environment.ApplicationName);
        using var activity = activitySource.StartActivity("Migrate database");

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DmsDbContext>();

            var pending = (await db.Database.GetPendingMigrationsAsync(stoppingToken)).ToList();
            await db.Database.MigrateAsync(stoppingToken);

            LogMigrated(logger, pending.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.AddException(ex);
            LogFailed(logger, ex);
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Database migrated; {Count} migration(s) applied")]
    private static partial void LogMigrated(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Database migration failed")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
