using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomEvaluationChangeEvent : RoomEvent<RoomEvaluationChangeEventPayload>
{
    public RoomEvaluationChangeEvent()
    {
        Type = EventType.RoomQuestionEvaluationModify;
    }
}

public sealed record RoomEvaluationChangeEventPayload(Guid QuestionId);
