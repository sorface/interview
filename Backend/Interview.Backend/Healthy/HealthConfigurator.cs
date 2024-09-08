using Interview.Domain.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Interview.Backend.Healthy;

public static class HealthConfigurator
{
    public static void Configure(IWebHostEnvironment environment, IServiceCollection serviceConfigurator, IConfiguration configuration)
    {
        serviceConfigurator.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database-health")
            .AddCheck("liveness", () => HealthCheckResult.Healthy());

        RedisEnvironmentConfigure.Configure(configuration, (host, port, username, password) =>
        {
            serviceConfigurator.AddHealthChecks().AddRedis($@"{host}:{port},password={password}");
        });
    }
}
