using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Tags;

namespace Interview.Domain.Questions;

public sealed class QuestionCreateRequest
{
    public string Value { get; set; } = string.Empty;

    public HashSet<Guid> Tags { get; set; } = new();

    public required EVQuestionType Type { get; set; }

    public required QuestionCodeEditorEditRequest? CodeEditor { get; set; }

    public required List<QuestionAnswerCreateRequest>? Answers { get; set; }
}
