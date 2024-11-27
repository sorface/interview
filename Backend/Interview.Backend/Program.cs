using Interview.Backend;
using Interview.Backend.AppEvents;
using Interview.Backend.Healthy;
using Interview.Backend.Logging;
using Interview.Domain.Database;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("oauth.json", true);
builder.Configuration.AddJsonFile("events.json", true);
builder.Configuration.AddEnvironmentVariables("INTERVIEW_BACKEND_");
builder.Configuration.AddJsonFile("initial.db.json", true);

// Add services to the container.
var serviceConfigurator = new ServiceConfigurator(builder.Environment, builder.Configuration);

LogConfigurator.Configure(builder.Host, builder.Services);
HealthConfigurator.Configure(builder.Environment, builder.Services, builder.Configuration);

serviceConfigurator.AddServices(builder.Services);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto, });

await MigrateDbAsync(app);

var middlewareConfigurator = new MiddlewareConfigurator(app);
middlewareConfigurator.AddMiddlewares();

app.Run();

async Task MigrateDbAsync(WebApplication webApplication)
{
    using var serviceScope = webApplication.Services.CreateScope();
    var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

    await appDbContext.Database.MigrateAsync();

    var applier = new EventApplier(app.Configuration);
    await applier.ApplyEventsAsync(appDbContext, CancellationToken.None);

    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<DevDbInitializer>>();
    var initializer = new DevDbInitializer(appDbContext, app.Configuration, logger);
    await initializer.InitializeAsync(CancellationToken.None);
}
