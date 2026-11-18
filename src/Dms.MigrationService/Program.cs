using Dms.Infrastructure.Persistence;
using Dms.MigrationService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// "dms" is the database resource name in the AppHost; Aspire passes it as ConnectionStrings__dms.
var connectionString = builder.Configuration.GetConnectionString("dms")
    ?? throw new InvalidOperationException("Connection string 'dms' is missing.");

builder.Services.AddDbContext<DmsDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHostedService<MigrationRunner>();

await builder.Build().RunAsync();
