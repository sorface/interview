namespace Interview.Domain.Rooms.Records.Response;

public class RoomCalendarItem
{
    public DateTime MinScheduledStartTime { get; set; }

    public List<EVRoomStatus> Statuses { get; set; } = [];
}
