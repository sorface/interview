using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomTimerStartEvent : RoomEvent<RoomTimerStartEventPayload>
{
    public RoomTimerStartEvent()
    {
        Type = EventType.StartRoomTimer;
    }
}

public sealed class RoomTimerStartEventPayload
{
    public double DurationSec { get; set; }

    public DateTime StartTime { get; set; }
}
