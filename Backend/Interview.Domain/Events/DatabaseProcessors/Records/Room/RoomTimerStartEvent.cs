using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomTimerStartEvent : RoomEvent<RoomTimerStartEventPayload>
{
    public RoomTimerStartEvent(Guid roomId, RoomTimerStartEventPayload? value, Guid createdById)
        : base(roomId, EventType.StartRoomTimer, value, false, createdById)
    {
    }
}

public sealed class RoomTimerStartEventPayload
{
    public double DurationSec { get; set; }

    public DateTime StartTime { get; set; }
}
