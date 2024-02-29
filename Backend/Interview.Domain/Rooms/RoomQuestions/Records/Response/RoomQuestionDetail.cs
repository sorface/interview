namespace Interview.Domain.Rooms.RoomQuestions.Records.Response;

public class RoomQuestionDetail
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Guid QuestionId { get; set; }

    public RoomQuestionState? State { get; set; }
}
