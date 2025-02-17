namespace Interview.Domain.Questions.QuestionTreeById;

public class QuestionTreeByIdResponseQuestionDetail
{
    public required Guid Id { get; set; }

    public required HashSet<string> Tags { get; set; }
}
