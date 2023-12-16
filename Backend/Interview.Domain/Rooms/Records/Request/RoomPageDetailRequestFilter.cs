namespace Interview.Domain.Rooms.Records.Request;

public class RoomPageDetailRequestFilter
{
    public string? Name { get; set; }

    public HashSet<EVRoomStatus>? Statuses { get; set; }

    public HashSet<Guid>? Participants { get; set; }
}
