using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Questions.Records.FindPage;

public class QuestionItem
{
    public Guid Id { get; init; }

    public string Value { get; init; } = string.Empty;

    public required List<TagItem> Tags { get; init; }
}
