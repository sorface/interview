using Interview.Domain.Tags;

namespace Interview.Domain.Questions;

public sealed class QuestionCreateRequest
{
    public string Value { get; set; } = string.Empty;

    public HashSet<Guid> Tags { get; set; } = new();
}
