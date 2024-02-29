using Interview.Backend;
using Interview.Backend.AppEvents;
using Interview.Backend.Logging;
using Interview.Domain.Events.Storage;
using Interview.Infrastructure.Database;
using Interview.Infrastructure.Events;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("oauth.json", true);
builder.Configuration.AddJsonFile("events.json", true);
builder.Configuration.AddEnvironmentVariables("INTERVIEW_BACKEND_");

LogConfigurator.Configure(builder.Host);

// Add services to the container.
var serviceConfigurator = new ServiceConfigurator(builder.Environment, builder.Configuration);
serviceConfigurator.AddServices(builder.Services);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto,
});

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

    var testUserId = Guid.Parse("b5a05f34-e44d-11ed-b49f-e8e34e3377ec");
    if (!webApplication.Environment.IsDevelopment() || appDbContext.Users.Any(e => e.Id == testUserId))
    {
        return;
    }

    await appDbContext.Users.AddAsync(new User("TEST_BACKEND_DEV_USER", "d1731c50-e44d-11ed-905c-d08c09609150")
    {
        Id = testUserId,
        Avatar = null,
        Roles =
        {
            appDbContext.Roles.Find(RoleName.User.Id)!,
        },
    });
    await appDbContext.SaveChangesAsync();
}
