using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomEvaluationChangeEvent : RoomEvent<RoomEvaluationChangeEventPayload>
{
    public RoomEvaluationChangeEvent(Guid roomId, RoomEvaluationChangeEventPayload payload, Guid createdById)
        : base(roomId, EventType.RoomQuestionEvaluationModify, payload, false, createdById)
    {
    }
}

public sealed record RoomEvaluationChangeEventPayload(Guid QuestionId);
