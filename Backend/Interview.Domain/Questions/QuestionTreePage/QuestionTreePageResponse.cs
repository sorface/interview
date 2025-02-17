namespace Interview.Domain.Questions.QuestionTreePage;

public class QuestionTreePageResponse
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required Guid? ParentQuestionTreeId { get; set; }
}
