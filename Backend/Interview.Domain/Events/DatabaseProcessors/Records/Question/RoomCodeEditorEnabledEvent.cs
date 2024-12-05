using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Question;

public class RoomCodeEditorEnabledEvent : RoomEvent<RoomCodeEditorEnabledEvent.Payload>
{
    public RoomCodeEditorEnabledEvent(Guid roomId, Payload? value, Guid createdById)
        : base(roomId, EventType.RoomCodeEditorEnabled, value, false, createdById)
    {
    }

    public class Payload
    {
        public bool Enabled { get; set; }
    }
}
