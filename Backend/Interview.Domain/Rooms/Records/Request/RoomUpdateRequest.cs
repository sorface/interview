namespace Interview.Domain.Rooms.Records.Request;

public class RoomUpdateRequest
{
    public string? Name { get; set; }

    public HashSet<Guid> Tags { get; set; } = [];

    public required List<RoomQuestionRequest> Questions { get; init; }

    public DateTime ScheduleStartTime { get; set; }

    public long? DurationSec { get; set; }

    public Guid? CategoryId { get; set; }
}
