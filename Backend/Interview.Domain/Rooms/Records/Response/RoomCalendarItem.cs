namespace Interview.Domain.Rooms.Records.Response;

public class RoomCalendarItem
{
    public DateTime MinScheduledStartTime { get; set; }

    public HashSet<EVRoomStatus> Statuses { get; set; } = new();
}
