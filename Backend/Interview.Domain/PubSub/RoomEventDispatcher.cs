using System.Text.Json;
using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.PubSub.Events;
using Interview.Domain.PubSub.Factory;

namespace Interview.Domain.PubSub;

public class RoomEventDispatcher : IRoomEventDispatcher
{
    private readonly IEventBusPublisherFactory _publisherFactory;

    public RoomEventDispatcher(IEventBusPublisherFactory publisherFactory)
    {
        _publisherFactory = publisherFactory;
    }

    public async Task WriteAsync(IRoomEvent @event, CancellationToken cancellationToken = default)
    {
        var publisher = await _publisherFactory.CreateAsync(cancellationToken);
        var eventAsBytes = JsonSerializer.SerializeToUtf8Bytes(@event);
        var sendAllInRoomEventBusEvent = new SendAllInRoomEventBusEvent
        {
            Event = eventAsBytes,
        };
        await publisher.PublishAsync(new EventBusRoomEventKey(@event.RoomId), sendAllInRoomEventBusEvent, cancellationToken);

        if (@event.Stateful)
        {
            var handleStatefulEventInRoomEvent = new ReceivedEventBusEvent { Event = eventAsBytes, };
            await publisher.PublishAsync(new HandleStatefulEventInRoomEventKey(), handleStatefulEventInRoomEvent, cancellationToken);
        }
    }
}
