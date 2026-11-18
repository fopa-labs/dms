using Dms.Core.Auditing;
using Dms.Core.Documents;
using Microsoft.EntityFrameworkCore;

namespace Dms.Infrastructure.Persistence;

/// <summary>
/// The core schema. Domain packs keep their tables in their own schema with their own DbContext.
/// </summary>
public sealed class DmsDbContext(DbContextOptions<DmsDbContext> options) : DbContext(options)
{
    public DbSet<Collection> Collections => Set<Collection>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    /// <summary>
    /// The snake_case naming is part of the model (the migrations depend on it), so it is set here
    /// rather than left to each place that registers the context.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSnakeCaseNamingConvention();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Installed in M0 so later migrations can rely on them: pgvector for embeddings (M4),
        // pg_trgm for fuzzy code search (M2–M3).
        modelBuilder
            .HasPostgresExtension("vector")
            .HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DmsDbContext).Assembly);
    }
}
