using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomTimerStartEvent : RoomEvent<RoomTimerStartEventPayload>
{
    public RoomTimerStartEvent(Guid roomId, RoomTimerStartEventPayload? value)
        : base(roomId, EventType.StartRoomTimer, value, false)
    {
    }
}

public sealed class RoomTimerStartEventPayload
{
    public double DurationSec { get; set; }

    public DateTime StartTime { get; set; }
}
