using Interview.Domain.Repository;

namespace Interview.Domain.Questions.QuestionAnswers;

public class QuestionAnswerResponse
{
    public static readonly Mapper<QuestionAnswer, QuestionAnswerResponse> Mapper = new(e => new QuestionAnswerResponse
    {
        Id = e.Id,
        Title = e.Title,
        Content = e.Content,
        CodeEditor = e.CodeEditor,
    });

    public required Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    public required bool CodeEditor { get; set; }
}
