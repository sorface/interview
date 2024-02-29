namespace Interview.Domain.Rooms.RoomQuestions.Records;

public class RoomQuestionsRequest
{
    public Guid RoomId { get; set; }

    public RoomQuestionStateType State { get; set; } = RoomQuestionStateType.Open;
}
