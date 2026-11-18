var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry, health checks, resilience and service discovery (Dms.ServiceDefaults).
builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Placeholder until the first real endpoint group.
app.MapGet("/", () => "Dms.Api");

await app.RunAsync();

// Exposes the implicit Program class to WebApplicationFactory<Program> in the integration tests.
public partial class Program;
