namespace Interview.Domain.Rooms.Records.Response;

public class RoomCalendarItem
{
    public DateTime TimeOffset { get; set; }

    public DateTime UtcTime { get; set; }

    public List<EVRoomStatus> Statuses { get; set; } = new();
}
