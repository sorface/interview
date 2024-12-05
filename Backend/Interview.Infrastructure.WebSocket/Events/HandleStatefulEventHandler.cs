using System.Text.Json;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.PubSub.Events;
using Interview.Domain.PubSub.Factory;
using Interview.Domain.Rooms.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events;

public class HandleStatefulEventHandler
{
    private readonly IEventBusSubscriberFactory _eventBusSubscriberFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IRoomEventSerializer _serializer;
    private readonly ILogger<HandleStatefulEventHandler> _logger;

    public HandleStatefulEventHandler(
        IEventBusSubscriberFactory eventBusSubscriberFactory,
        IServiceScopeFactory serviceScopeFactory,
        IRoomEventSerializer serializer,
        ILogger<HandleStatefulEventHandler> logger)
    {
        _eventBusSubscriberFactory = eventBusSubscriberFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task AddHandlerAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Before subscribe to event bus");
        try
        {
            var subscriber = await _eventBusSubscriberFactory.CreateAsync(cancellationToken);
            await subscriber.SubscribeAsync(
                new HandleStatefulEventInRoomEventKey(),
                (_, @event) =>
                {
                    if (@event is null)
                    {
                        return;
                    }

                    var task = @event.Match<Task>(
                        HandleReceiveEventAsync,
                        busEvent => Task.CompletedTask);
                    task.ConfigureAwait(false).GetAwaiter().GetResult();
                },
                cancellationToken);

            _logger.LogInformation("After subscribe to event bus");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "During subscribe to event bus");
        }
    }

    private async Task HandleReceiveEventAsync(ReceivedEventBusEvent busEvent)
    {
        try
        {
            var roomEvent = JsonSerializer.Deserialize<IRoomEvent>(busEvent.Event) ?? throw new Exception("Unable to parse event");
            await using var dbScope = _serviceScopeFactory.CreateAsyncScope();
            var service = dbScope.ServiceProvider.GetRequiredService<IRoomServiceWithoutPermissionCheck>();
            try
            {
                var payload = roomEvent.BuildStringPayload(_serializer);
                await service.UpsertRoomStateAsync(
                    roomEvent.RoomId,
                    roomEvent.Type,
                    payload ?? string.Empty,
                    CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "During update {Type} room state", roomEvent.Type);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fails to update room states");
        }
    }
}
