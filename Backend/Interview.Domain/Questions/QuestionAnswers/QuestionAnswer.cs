using Interview.Domain.Repository;

namespace Interview.Domain.Questions.QuestionAnswers;

public class QuestionAnswer : Entity
{
    public required string Title { get; set; }

    public required string Content { get; set; }

    public required bool CodeEditor { get; set; }

    public required Guid QuestionId { get; set; }

    public required Question? Question { get; set; }

    public static void EnsureValid(
        IEnumerable<Validate>? answers,
        bool isAvailableCodeEditor)
    {
        if (answers is null)
        {
            return;
        }

        if (isAvailableCodeEditor)
        {
            return;
        }

        if (answers.Any(e => e.CodeEditor))
        {
            throw new UserException("Code editor is not available for answer.");
        }
    }

    public record Validate(bool CodeEditor)
    {
        public Validate(QuestionAnswerEditRequest a)
            : this(a.CodeEditor)
        {
        }

        public Validate(QuestionAnswerCreateRequest a)
            : this(a.CodeEditor)
        {
        }
    }
}
