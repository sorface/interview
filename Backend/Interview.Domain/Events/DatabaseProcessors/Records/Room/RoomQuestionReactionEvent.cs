using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionReactionEvent : RoomEvent<RoomQuestionReactionPayload>
{
    public RoomQuestionReactionEvent(Guid roomId, string type, RoomQuestionReactionPayload? value)
        : base(roomId, type, value, false)
    {
    }
}

public sealed class RoomQuestionReactionPayload
{
    public Guid UserId { get; }

    public string? Payload { get; }

    public RoomQuestionReactionPayload(Guid userId, string? payload)
    {
        UserId = userId;
        Payload = payload;
    }
}
