using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Interview.Domain.Events.Events;
using Interview.Domain.PubSub.Events;
using Interview.Domain.PubSub.Factory;

namespace Interview.Domain.Events;

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
        var sendAllInRoomEventBusEvent = new SendAllInRoomEventBusEvent
        {
            Event = JsonSerializer.SerializeToUtf8Bytes(@event),
        };
        await publisher.PublishAsync(new EventBusRoomEventKey(@event.RoomId), sendAllInRoomEventBusEvent, cancellationToken);
    }
}
