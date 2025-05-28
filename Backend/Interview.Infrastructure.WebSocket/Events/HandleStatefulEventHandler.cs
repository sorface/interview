using System.Text.Json;
using Interview.Domain;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.PubSub.Events;
using Interview.Domain.PubSub.Factory;
using Interview.Domain.Rooms.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events;

public class HandleStatefulEventHandler(
    IEventBusSubscriberFactory eventBusSubscriberFactory,
    IServiceScopeFactory serviceScopeFactory,
    IEventSerializer serializer,
    IEventDeserializer deserializer,
    ILogger<HandleStatefulEventHandler> logger)
{
    public async Task AddHandlerAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Before subscribe to event bus");
        try
        {
            var subscriber = await eventBusSubscriberFactory.CreateAsync(cancellationToken);
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

            logger.LogInformation("After subscribe to event bus");
        }
        catch (Exception e)
        {
            logger.LogError(e, "During subscribe to event bus");
        }
    }

    private async Task HandleReceiveEventAsync(ReceivedEventBusEvent busEvent)
    {
        try
        {
            var roomEvent = deserializer.Deserialize<IRoomEvent>(busEvent.Event) ?? throw new UserException("Unable to parse event");
            await using var dbScope = serviceScopeFactory.CreateAsyncScope();
            var service = dbScope.ServiceProvider.GetRequiredService<IRoomServiceWithoutPermissionCheck>();
            try
            {
                var payload = roomEvent.BuildStringPayload(serializer);
                await service.UpsertRoomStateAsync(
                    roomEvent.RoomId,
                    roomEvent.Type,
                    payload ?? string.Empty,
                    CancellationToken.None);
            }
            catch (Exception e)
            {
                logger.LogError(e, "During update {Type} room state", roomEvent.Type);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Fails to update room states");
        }
    }
}
