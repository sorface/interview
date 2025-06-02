namespace Interview.Domain.Rooms.BusinessAnalytic;

public class BusinessAnalyticRequestFilter
{
    public required DateTime StartDate { get; set; }

    public required DateTime EndDate { get; set; }

    public HashSet<EVRoomType>? RoomTypes { get; set; }

    public EVRoomAccessType? AccessType { get; set; }
}
