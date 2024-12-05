using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomEvaluationAddEvent : RoomEvent<RoomEvaluationAddEventPayload>
{
    public RoomEvaluationAddEvent()
    {
        Type = EventType.RoomQuestionEvaluationAdded;
    }
}

public sealed record RoomEvaluationAddEventPayload(Guid QuestionId);
