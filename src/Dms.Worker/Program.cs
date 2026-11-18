var builder = Host.CreateApplicationBuilder(args);

// OpenTelemetry, health checks, resilience and service discovery (Dms.ServiceDefaults).
builder.AddServiceDefaults();

// The job-processing BackgroundService is registered here in S4.

var host = builder.Build();
await host.RunAsync();
