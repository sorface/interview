namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomCreateRequest
{
    public string Name { get; set; } = string.Empty;

    public SERoomAccessType AccessType { get; set; } = SERoomAccessType.Public;

    public required List<RoomQuestionRequest> Questions { get; init; }

    public required HashSet<Guid> Experts { get; init; }

    public required HashSet<Guid> Examinees { get; init; }

    public required HashSet<Guid> Tags { get; init; }

    public long? DurationSec { get; set; }

    public DateTime ScheduleStartTime { get; set; }

    public Guid? QuestionTreeId { get; set; }
}
