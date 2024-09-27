namespace Interview.Domain.Rooms.Records.Response;

public class RoomCalendarResponse
{
    public List<RoomCalendarItem> MeetingSchedules { get; init; } = new();
}
