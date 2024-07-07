namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;

public class QuestionEvaluationMergeRequest
{
    public Guid RoomId { get; set; }

    public Guid QuestionId { get; set; }

    public string? Review { get; set; }

    public int? Mark { get; set; }

    public Guid UserId { get; set; }
}
