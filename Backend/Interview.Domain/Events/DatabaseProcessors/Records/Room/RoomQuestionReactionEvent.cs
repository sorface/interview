using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionReactionEvent : RoomEvent<RoomQuestionReactionPayload>
{
    public RoomQuestionReactionEvent(Guid roomId, string type, RoomQuestionReactionPayload? value, Guid createdById)
        : base(roomId, type, value, false, createdById)
    {
    }
}

public sealed class RoomQuestionReactionPayload
{
    public required Guid UserId { get; init; }

    public required string? Payload { get; init; }
}
