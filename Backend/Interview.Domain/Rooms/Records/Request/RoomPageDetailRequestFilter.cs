namespace Interview.Domain.Rooms.Records.Request;

public class RoomPageDetailRequestFilter
{
    public string? Name { get; set; }

    public HashSet<EVRoomStatus>? Statuses { get; set; }

    public HashSet<Guid>? Participants { get; set; }

    /// <summary>
    /// Gets or sets filtering of the scheduled room time starts from the set time (scheduled.time >= StartValue)
    /// </summary>
    public DateTime? StartValue { get; set; }

    /// <summary>
    /// Gets or sets filtering the scheduled room time ends before (scheduled.time <= EndValue) the set time
    /// </summary>
    public DateTime? EndValue { get; set; }
}
