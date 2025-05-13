using Interview.Domain.Categories.Page;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Repository;
using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Questions.Records.FindPage;

public class QuestionItem
{
    public static readonly Mapper<Question, QuestionItem> Mapper = new(e => new QuestionItem
    {
        Id = e.Id,
        Value = e.Value,
        Tags = e.Tags.Select(t => new TagItem
        {
            Id = t.Id,
            Value = t.Value,
            HexValue = t.HexColor,
        })
            .ToList(),
        Category = e.Category != null
            ? new CategoryResponse
            {
                Id = e.Category.Id,
                Name = e.Category.Name,
                ParentId = e.Category.ParentId,
                Order = e.Category.Order,
            }
            : null,
        Answers = e.Answers.Select(q => new QuestionAnswerResponse
        {
            Id = q.Id,
            Title = q.Title,
            Content = q.Content,
            CodeEditor = q.CodeEditor,
        })
            .ToList(),
        CodeEditor = e.CodeEditor == null
            ? null
            : new QuestionCodeEditorResponse
            {
                Content = e.CodeEditor.Content,
                Lang = e.CodeEditor.Lang,
            },
        Author = e.CreatedBy == null
            ? null
            : new QuestionItemAuthorResponse
            {
                UserId = e.CreatedBy.Id,
                Nickname = e.CreatedBy.Nickname,
            },
    });

    public Guid Id { get; init; }

    public string Value { get; init; } = string.Empty;

    public required List<TagItem> Tags { get; init; }

    public required List<QuestionAnswerResponse> Answers { get; init; }

    public required QuestionCodeEditorResponse? CodeEditor { get; init; }

    public required CategoryResponse? Category { get; init; }

    public required QuestionItemAuthorResponse? Author { get; init; }
}
