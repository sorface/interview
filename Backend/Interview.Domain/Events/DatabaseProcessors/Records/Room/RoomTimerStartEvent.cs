using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomTimerStartEvent : RoomEvent<RoomTimerPayload>
{
    public RoomTimerStartEvent(Guid roomId, RoomTimerPayload? value)
        : base(roomId, EventType.StartRoomTimer, value, false)
    {
    }
}

public sealed class RoomTimerPayload
{
    public long Duration { get; set; }

    public long StartTime { get; set; }
}
