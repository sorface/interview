namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record SendAllInRoomRedisRoomEvent : SendRedisRoomEvent
{
    public override T Match<T>(
        Func<ReceivedRedisRoomEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomRedisRoomEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomRedisRoomEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomRedisRoomEvent, T> sendSpecificParticipantRoomRedisRoomEvent)
    {
        return sendAllInRoomRedisRoomEvent(this);
    }
}
