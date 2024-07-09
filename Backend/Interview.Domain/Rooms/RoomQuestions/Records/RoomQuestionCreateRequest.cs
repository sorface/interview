using Interview.Domain.Questions;

namespace Interview.Domain.Rooms.RoomQuestions.Records;

public class RoomQuestionCreateRequest
{
    public Guid RoomId { get; set; }

    public Guid? QuestionId { get; set; }

    public QuestionCreateRequest? Question { get; set; }

    public int Order { get; set; }
}
