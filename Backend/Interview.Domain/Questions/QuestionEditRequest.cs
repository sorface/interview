using Interview.Domain.Questions.QuestionAnswers;

namespace Interview.Domain.Questions;

public sealed class QuestionEditRequest
{
    public string Value { get; set; } = string.Empty;

    public HashSet<Guid> Tags { get; set; } = new();

    // public required bool CodeEditor { get; set; }
    public required List<QuestionAnswerCreateRequest>? NewAnswers { get; set; }

    public required HashSet<QuestionAnswerEditRequest>? ExistsAnswers { get; set; }
}
