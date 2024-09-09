using Interview.Domain.Events;

namespace Interview.Backend;

public class EventStorage2DatabaseBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EventStorage2DatabaseBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = _scopeFactory.CreateAsyncScope())
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

            await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
        }
    }
}
