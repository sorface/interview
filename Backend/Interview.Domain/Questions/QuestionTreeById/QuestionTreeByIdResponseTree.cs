namespace Interview.Domain.Questions.QuestionTreeById;

public class QuestionTreeByIdResponseTree
{
    public required Guid Id { get; set; }

    public required EVQuestionSubjectTreeType Type { get; set; }

    public required Guid? ParentQuestionSubjectTreeId { get; set; }

    public required QuestionTreeByIdResponseQuestionDetail Question { get; set; }
}
