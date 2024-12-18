using System.Text.Json;

namespace Interview.Domain.PubSub.Events;

[JsonDerivedTypeByName<ReceivedEventBusEvent>]
[JsonDerivedTypeByName<SendAllInRoomEventBusEvent>]
public abstract record EventBusEvent
{
    public required byte[] Event { get; set; }

    public abstract T Match<T>(
        Func<ReceivedEventBusEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomEventBusEvent, T> sendAllInRoomRedisRoomEvent);
}
