using Ardalis.SmartEnum;

namespace Interview.Domain.RoomParticipants;

public enum EVRoomParticipantType
{
    /// <summary>
    /// Viewer.
    /// </summary>
    Viewer = 1,

    /// <summary>
    /// Expert.
    /// </summary>
    Expert,

    /// <summary>
    /// Examinee.
    /// </summary>
    Examinee,
}
