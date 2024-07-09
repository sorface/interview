using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomEvaluationAddEvent : RoomEvent<RoomEvaluationAddEventPayload>
{
    public RoomEvaluationAddEvent(Guid roomId, RoomEvaluationAddEventPayload payload)
        : base(roomId, EventType.RoomQuestionEvaluationAdded, payload, false)
    {
    }
}

public sealed record RoomEvaluationAddEventPayload(Guid QuestionId);
