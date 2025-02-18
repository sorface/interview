namespace Interview.Domain.Questions.UpsertQuestionTree;

public class UpsertQuestionSubjectTreeRequest
{
    public Guid Id { get; set; }

    public Guid? ParentQuestionSubjectTreeId { get; set; }

    public required Guid QuestionId { get; set; }

    public required EVQuestionSubjectTreeType Type { get; set; }

    public required int Order { get; set; }
}
