namespace Interview.Domain.PubSub.Events;

public record ReceivedEventBusEvent : EventBusEvent
{
    public override T Match<T>(
        Func<ReceivedEventBusEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomEventBusEvent, T> sendAllInRoomRedisRoomEvent)
    {
        return receiveRedisRoomEvent(this);
    }
}
