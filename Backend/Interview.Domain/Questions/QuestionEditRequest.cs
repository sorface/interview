using Interview.Domain.Tags;

namespace Interview.Domain.Questions;

public sealed class QuestionEditRequest
{
    public string Value { get; set; } = string.Empty;

    public HashSet<Guid> Tags { get; set; } = new();

    public required Guid? CategoryId { get; init; }
}
