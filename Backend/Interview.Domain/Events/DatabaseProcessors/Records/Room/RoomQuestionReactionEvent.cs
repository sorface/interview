using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionReactionEvent : RoomEvent<RoomQuestionReactionPayload>
{
    public RoomQuestionReactionEvent(Guid roomId, string type, RoomQuestionReactionPayload? value)
        : base(roomId, type, value, false)
    {
    }
}
