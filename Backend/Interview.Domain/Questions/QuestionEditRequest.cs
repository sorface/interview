using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;

namespace Interview.Domain.Questions;

public sealed class QuestionEditRequest
{
    public string Value { get; set; } = string.Empty;

    public EVQuestionType? Type { get; set; }

    public HashSet<Guid> Tags { get; set; } = [];

    public required Guid? CategoryId { get; init; }

    public required QuestionCodeEditorEditRequest? CodeEditor { get; set; }

    public required List<QuestionAnswerEditRequest>? Answers { get; set; }
}
