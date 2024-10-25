using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record SendSpecificParticipantRoomEventBusEvent : SendEventBusEvent
{
    public required HashSet<EVRoomParticipantType> Participant { get; set; }

    public override T Match<T>(
        Func<ReceivedEventBusEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomEventBusEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomEventBusEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomEventBusEvent, T> sendSpecificParticipantRoomRedisRoomEvent)
    {
        return sendSpecificParticipantRoomRedisRoomEvent(this);
    }
}
