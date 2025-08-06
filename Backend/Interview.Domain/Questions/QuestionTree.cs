using Interview.Domain.Repository;

namespace Interview.Domain.Questions;

public sealed class QuestionTree : ArchiveEntity
{
    public required string Name { get; set; }

    public string? ThemeAiDescription { get; set; }

    public required Guid RootQuestionSubjectTreeId { get; set; }

    public QuestionSubjectTree? RootQuestionSubjectTree { get; set; }

    public Guid? ParentQuestionTreeId { get; set; }

    public QuestionTree? ParentQuestionTree { get; set; }

    public int Order { get; set; }
}
