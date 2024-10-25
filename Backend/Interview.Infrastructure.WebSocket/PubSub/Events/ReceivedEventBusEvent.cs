namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record ReceivedEventBusEvent : EventBusEvent
{
    public override T Match<T>(
        Func<ReceivedEventBusEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomEventBusEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomEventBusEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomEventBusEvent, T> sendSpecificParticipantRoomRedisRoomEvent)
    {
        return receiveRedisRoomEvent(this);
    }
}
