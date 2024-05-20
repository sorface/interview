namespace Interview.Domain.Rooms.RoomQuestions.Records;

public class RoomQuestionChangeActiveRequest
{
    public Guid RoomId { get; set; }

    public Guid QuestionId { get; set; }
}
