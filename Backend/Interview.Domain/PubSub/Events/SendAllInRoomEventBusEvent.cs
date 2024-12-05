namespace Interview.Domain.PubSub.Events;

public record SendAllInRoomEventBusEvent : SendEventBusEvent
{
    public override T Match<T>(
        Func<ReceivedEventBusEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomEventBusEvent, T> sendAllInRoomRedisRoomEvent)
    {
        return sendAllInRoomRedisRoomEvent(this);
    }
}
