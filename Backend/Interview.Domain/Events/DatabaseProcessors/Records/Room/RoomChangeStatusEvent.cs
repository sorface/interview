using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomChangeStatusEvent : RoomEvent
{
    public RoomChangeStatusEvent(Guid roomId, string? value)
        : base(roomId, EventType.ChangeRoomStatus, value, false)
    {
    }
}
