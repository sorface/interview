namespace Interview.Domain.Events.DatabaseProcessors.Records.Question;

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
