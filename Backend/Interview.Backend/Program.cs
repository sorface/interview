using Interview.Backend;
using Interview.Backend.AppEvents;
using Interview.Backend.Healthy;
using Interview.Backend.Logging;
using Interview.Domain.Database;
using Interview.Infrastructure.WebSocket.Events;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("oauth.json", true);
builder.Configuration.AddJsonFile("events.json", true);
builder.Configuration.AddEnvironmentVariables("INTERVIEW_BACKEND_");
builder.Configuration.AddJsonFile("initial.db.json", true);

LogConfigurator.Configure(builder.Host, builder.Services);
HealthConfigurator.Configure(builder.Environment, builder.Services, builder.Configuration);

using (var cmdLogger = new LoggerConfiguration()
           .WriteTo.Console()
           .CreateLogger())
{
    // Add services to the container.
    var serviceConfigurator = new ServiceConfigurator(builder.Environment, builder.Configuration, cmdLogger.ForContext<ServiceConfigurator>());
    serviceConfigurator.AddServices(builder.Services);
}

var app = builder.Build();

app.Logger.LogInformation("Build web application {Environment}", app.Environment.EnvironmentName);

try
{
    var handleStatefulEventHandler = app.Services.GetRequiredService<HandleStatefulEventHandler>();
    await handleStatefulEventHandler.AddHandlerAsync(CancellationToken.None);

    await MigrateDbAsync(app);

    app.Logger.LogInformation("Add middlewares...");
    var middlewareConfigurator = new MiddlewareConfigurator(app);
    middlewareConfigurator.AddMiddlewares();

    app.Logger.LogInformation("Application starting...");
    await app.RunAsync();
}
catch (Exception e)
{
    app.Logger.LogError(e, "During run app");
    throw;
}
finally
{
    app.Logger.LogInformation("Application stopped...");
}

async Task MigrateDbAsync(WebApplication webApplication)
{
    webApplication.Logger.LogInformation("Apply migrations...");
    using var serviceScope = webApplication.Services.CreateScope();
    var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

    await appDbContext.Database.MigrateAsync();

    webApplication.Logger.LogInformation("Apply InitialEvents...");
    var applier = new EventApplier(app.Configuration);
    await applier.ApplyEventsAsync(appDbContext, CancellationToken.None);

    webApplication.Logger.LogInformation("Apply InitialDb...");
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<DevDbInitializer>>();
    var initializer = new DevDbInitializer(appDbContext, app.Configuration, logger);
    await initializer.InitializeAsync(CancellationToken.None);
}
