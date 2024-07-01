namespace Interview.Domain.Questions.QuestionAnswers;

public class QuestionAnswerEditRequest
{
    public required Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    public required bool CodeEditor { get; set; }
}
