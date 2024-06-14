using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Events.Question;

public class QuestionChangeEvent : RoomEvent<QuestionChangeEventPayload>
{
    public QuestionChangeEvent(Guid roomId, QuestionChangeEventPayload? value)
        : base(roomId, EventType.ChangeQuestion, value, false)
    {
    }
}

public sealed class QuestionChangeEventPayload
{
    public Guid QuestionId { get; }

    public string OldValue { get; }

    public string NewValue { get; }

    public QuestionChangeEventPayload(Guid questionId, string oldValue, string newValue)
    {
        QuestionId = questionId;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
