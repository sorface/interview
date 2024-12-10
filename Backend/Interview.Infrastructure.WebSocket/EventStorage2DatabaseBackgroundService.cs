using Interview.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Interview.Infrastructure.WebSocket;

public class EventStorage2DatabaseBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = scopeFactory.CreateAsyncScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<EventStorage2DatabaseService>();
                try
                {
                    await service.ProcessAsync(stoppingToken);
                }
                catch
                {
                    // ignore
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
