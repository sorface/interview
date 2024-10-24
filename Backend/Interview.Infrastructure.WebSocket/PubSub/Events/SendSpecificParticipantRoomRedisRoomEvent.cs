using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record SendSpecificParticipantRoomRedisRoomEvent : SendRedisRoomEvent
{
    public required HashSet<EVRoomParticipantType> Participant { get; set; }

    public override T Match<T>(
        Func<ReceivedRedisRoomEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomRedisRoomEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomRedisRoomEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomRedisRoomEvent, T> sendSpecificParticipantRoomRedisRoomEvent)
    {
        return sendSpecificParticipantRoomRedisRoomEvent(this);
    }
}
