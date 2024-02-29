namespace Interview.Domain.Rooms.RoomQuestions.Records;

public class RoomQuestionCreateRequest
{
    public Guid RoomId { get; set; }

    public Guid QuestionId { get; set; }
}