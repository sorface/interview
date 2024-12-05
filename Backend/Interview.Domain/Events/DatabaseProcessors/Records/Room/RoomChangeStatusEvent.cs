using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomChangeStatusEvent : RoomEvent
{
    public RoomChangeStatusEvent()
    {
        Type = EventType.ChangeRoomStatus;
    }
}
