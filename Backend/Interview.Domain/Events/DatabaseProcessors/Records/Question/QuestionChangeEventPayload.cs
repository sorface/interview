namespace Interview.Domain.Events.DatabaseProcessors.Records.Question;

public sealed class QuestionChangeEventPayload(Guid questionId, string oldValue, string newValue)
{
    public Guid QuestionId { get; } = questionId;

    public string OldValue { get; } = oldValue;

    public string NewValue { get; } = newValue;
}
