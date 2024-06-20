namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public sealed class RoomTimerStartEventPayload
{
    public long Duration { get; set; }

    public long StartTime { get; set; }
}
