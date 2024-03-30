using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms.Records.Response;

/// <summary>
/// Room invite.
/// </summary>
public class RoomInviteResponse
{
    /// <summary>
    /// Gets or sets invite id.
    /// </summary>
    public required Guid InviteId { get; set; }

    /// <summary>
    /// Gets or sets participant type.
    /// </summary>
    public required EVRoomParticipantType ParticipantType { get; set; }
}
