var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Placeholder until the first real endpoint group; health checks come with ServiceDefaults.
app.MapGet("/", () => "Dms.Api");

await app.RunAsync();

// Exposes the implicit Program class to WebApplicationFactory<Program> in the integration tests.
public partial class Program;
