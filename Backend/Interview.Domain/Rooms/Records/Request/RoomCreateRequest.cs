namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomCreateRequest
{
    public string Name { get; set; } = string.Empty;

    public SERoomAccessType AccessType { get; set; } = SERoomAccessType.Public;

    public required HashSet<Question> Questions { get; init; }

    public required HashSet<Guid> Experts { get; init; }

    public required HashSet<Guid> Examinees { get; init; }

    public required HashSet<Guid> Tags { get; init; }

    public long? DurationSec { get; set; }

    public class Question
    {
        public required Guid Id { get; init; }

        public required int Order { get; init; }
    }
}
