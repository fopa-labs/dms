var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL with pgvector. The tag is pinned so every developer and CI run gets the same extension
// version. The data volume and the persistent lifetime keep the database across AppHost restarts.
var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector", "0.8.7-pg17")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("dms");

// Applies the EF Core migrations, then exits.
var migrations = builder.AddProject<Projects.Dms_MigrationService>("migrations")
    .WithReference(database)
    .WaitFor(database);

// Api and Worker start only after the migrations have completed successfully.
builder.AddProject<Projects.Dms_Api>("api")
    .WithReference(database)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.Dms_Worker>("worker")
    .WithReference(database)
    .WaitForCompletion(migrations);

await builder.Build().RunAsync();
