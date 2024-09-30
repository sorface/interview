namespace Interview.Domain.Rooms.Records.Request;

public class RoomCalendarRequest
{
    public DateTime? StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public HashSet<EVRoomStatus>? RoomStatus { get; set; } = new();

    public required int TimeZoneOffset { get; set; } = 0;
}
