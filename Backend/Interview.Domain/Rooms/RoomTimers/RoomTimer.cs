using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomTimers;

/// <summary>
/// Room Timer.
/// </summary>
public class RoomTimer : Entity
{
    /// <summary>
    /// Duration timer.
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// the actual start time of the timer.
    /// </summary>
    public DateTime? ActualStartTime { get; set; }
    
    /// <summary>
    /// Timer Room.
    /// </summary>
    public Room? Room { get; set; }
}
