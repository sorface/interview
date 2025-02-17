namespace Interview.Domain.Questions.QuestionTreeById;

public class QuestionTreeByIdResponse
{
    public required string Name { get; set; }

    public required List<QuestionTreeByIdResponseQuestionDetail> QuestionDetail { get; set; }

    public required List<QuestionTreeByIdResponseTree> Tree { get; set; }
}
