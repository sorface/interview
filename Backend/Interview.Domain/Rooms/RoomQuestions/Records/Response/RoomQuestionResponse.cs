namespace Interview.Domain.Rooms.RoomQuestions.Records.Response;

public class RoomQuestionResponse
{
    public Guid Id { get; set; }

    public string? Value { get; set; }

    public RoomQuestionStateType? State { get; set; }
}
