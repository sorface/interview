namespace Interview.Backend.RoomEvaluations.Records;

public class QuestionEvaluationMergeRequestApi
{
    public Guid RoomId { get; set; }

    public Guid QuestionId { get; set; }

    public string? Review { get; set; }

    public int? Mark { get; set; }
}

