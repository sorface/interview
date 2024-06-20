using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomTimers;

/// <summary>
/// Room Timer.
/// </summary>
public class RoomTimer : Entity
{
    /// <summary>
    /// Gets or sets duration timer.
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// Gets or sets the actual start time of the timer.
    /// </summary>
    public DateTime? ActualStartTime { get; set; }

    /// <summary>
    /// Gets or sets timer Room.
    /// </summary>
    public Room? Room { get; set; }
}
