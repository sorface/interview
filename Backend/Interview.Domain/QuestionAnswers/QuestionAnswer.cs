using Interview.Domain.Questions;
using Interview.Domain.Repository;

namespace Interview.Domain.QuestionAnswers;

public class QuestionAnswer : Entity
{
    public required string Title { get; set; }

    public required string Content { get; set; }

    public required string CodeEditor { get; set; }

    public required Guid QuestionId { get; set; }

    public required Question? Question { get; set; }
}
