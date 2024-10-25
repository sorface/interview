namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record SendSpecificUsersRoomEventBusEvent : SendEventBusEvent
{
    public required HashSet<Guid> Users { get; set; }

    public override T Match<T>(
        Func<ReceivedEventBusEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomEventBusEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomEventBusEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomEventBusEvent, T> sendSpecificParticipantRoomRedisRoomEvent)
    {
        return sendSpecificUsersRoomRedisRoomEvent(this);
    }
}
