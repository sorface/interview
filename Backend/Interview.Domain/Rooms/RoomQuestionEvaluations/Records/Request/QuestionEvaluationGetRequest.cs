namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;

public class QuestionEvaluationGetRequest
{
    public Guid RoomId { get; set; }

    public Guid QuestionId { get; set; }

    public Guid UserId { get; set; }
}
