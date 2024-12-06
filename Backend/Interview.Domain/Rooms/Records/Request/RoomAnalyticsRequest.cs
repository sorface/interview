namespace Interview.Domain.Rooms.Records.Request;

public class RoomAnalyticsRequest(Guid roomId)
{
    public Guid RoomId { get; } = roomId;
}
