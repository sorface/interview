using Interview.Domain.Rooms.Records.Request;

namespace Interview.Backend.Rooms;

/// <summary>
/// Room Create Api Request.
/// </summary>
public class RoomCreateApiRequest
{
    public string Name { get; set; } = string.Empty;

    public string AccessType { get; set; } = SERoomAccessType.Public.Name;

    public List<RoomQuestionRequest> Questions { get; set; } = [];

    public HashSet<Guid> Experts { get; set; } = [];

    public HashSet<Guid> Examinees { get; set; } = [];

    public HashSet<Guid> Tags { get; set; } = [];

    public long? Duration { get; set; }

    public DateTime ScheduleStartTime { get; set; }

    public Guid? QuestionTreeId { get; set; }
}
