namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;

public class QuestionEvaluationDetail
{
    public required Guid Id { get; init; }

    public int? Mark { get; set; }

    public string? Review { get; set; }
}
