using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;

namespace Interview.Domain.Questions;

public sealed class QuestionCreateRequest
{
    public string Value { get; set; } = string.Empty;

    public HashSet<Guid> Tags { get; set; } = new();

    public required EVQuestionType Type { get; set; }

    public required Guid? CategoryId { get; init; }

    public required QuestionCodeEditorEditRequest? CodeEditor { get; set; }

    public required List<QuestionAnswerCreateRequest>? Answers { get; set; }
}
