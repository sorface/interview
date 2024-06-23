using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomCodeEditorChangeEvent : RoomEvent
{
    public RoomCodeEditorChangeEvent(Guid roomId, string? value)
        : base(roomId, EventType.ChangeCodeEditor, value, false)
    {
    }
}
