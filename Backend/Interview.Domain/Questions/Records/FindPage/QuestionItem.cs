using Interview.Domain.Categories.Page;
using Interview.Domain.Repository;
using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Questions.Records.FindPage;

public class QuestionItem
{
    public static readonly Mapper<Question, QuestionItem> Mapper = new(e => new QuestionItem
    {
        Id = e.Id,
        Value = e.Value,
        Tags = e.Tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
        Category = e.Category != null
            ? new CategoryResponse { Id = e.Category.Id, Name = e.Category.Name, ParentId = e.Category.ParentId, }
            : null,
    });

    public Guid Id { get; init; }

    public string Value { get; init; } = string.Empty;

    public required List<TagItem> Tags { get; init; }

    public required CategoryResponse? Category { get; init; }
}
