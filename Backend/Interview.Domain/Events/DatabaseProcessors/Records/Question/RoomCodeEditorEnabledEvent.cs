using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Question;

public class RoomCodeEditorEnabledEvent : RoomEvent<RoomCodeEditorEnabledEvent.Payload>
{
    public RoomCodeEditorEnabledEvent()
    {
        Type = EventType.RoomCodeEditorEnabled;
    }

    public class Payload
    {
        public bool Enabled { get; set; }
    }
}
