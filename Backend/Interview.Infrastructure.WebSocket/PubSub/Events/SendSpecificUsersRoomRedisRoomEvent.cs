namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record SendSpecificUsersRoomRedisRoomEvent : SendRedisRoomEvent
{
    public required HashSet<Guid> Users { get; set; }

    public override T Match<T>(
        Func<ReceivedRedisRoomEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomRedisRoomEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomRedisRoomEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomRedisRoomEvent, T> sendSpecificParticipantRoomRedisRoomEvent)
    {
        return sendSpecificUsersRoomRedisRoomEvent(this);
    }
}
