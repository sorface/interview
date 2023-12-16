namespace Interview.Domain.Rooms.Records.Request;

public class RoomAnalyticsRequest
{
    public Guid RoomId { get; }

    public RoomAnalyticsRequest(Guid roomId)
    {
        RoomId = roomId;
    }
}
