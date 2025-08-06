using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomExpireServices;

namespace Interview.Backend.BackgroundServices;

public class RoomExpireServiceBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = scopeFactory.CreateAsyncScope())
            {
                var userAccessor = scope.ServiceProvider.GetRequiredService<IEditableCurrentUserAccessor>();
                userAccessor.SetUser(User.Backend);
                var service = scope.ServiceProvider.GetRequiredService<RoomExpireService>();
                try
                {
                    await service.ProcessAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
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
