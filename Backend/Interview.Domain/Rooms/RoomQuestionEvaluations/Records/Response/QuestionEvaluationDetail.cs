namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;

public class QuestionEvaluationDetail
{
    public QuestionEvaluationDetail(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }

    public int? Mark { get; set; }

    public string? Review { get; set; }
}
