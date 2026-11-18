var builder = Host.CreateApplicationBuilder(args);

// The job-processing BackgroundService is registered here in S4.

var host = builder.Build();
await host.RunAsync();
