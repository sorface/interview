namespace Interview.Domain.Questions.QuestionAnswers;

public class QuestionAnswerCreateRequest
{
    public required string Title { get; set; }

    public required string Content { get; set; }

    public required bool CodeEditor { get; set; }
}
