namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;

public class RoomQuestionEvaluationResponse
{
    public required Guid Id { get; set; }

    public required string Value { get; set; }

    public required int Order { get; set; }

    public required QuestionEvaluationDetail? Evaluation { get; set; }
}
