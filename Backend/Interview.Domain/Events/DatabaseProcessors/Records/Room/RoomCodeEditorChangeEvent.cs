using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomConfigurations;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomCodeEditorChangeEvent : RoomEvent<RoomCodeEditorChangeEvent.Payload>
{
    public RoomCodeEditorChangeEvent(Guid roomId, Payload? value, Guid createdById)
        : base(roomId, EventType.ChangeCodeEditor, value, false, createdById)
    {
    }

    public sealed class Payload
    {
        public required string? Content { get; set; }

        public required EVRoomCodeEditorChangeSource Source { get; set; }
    }
}
