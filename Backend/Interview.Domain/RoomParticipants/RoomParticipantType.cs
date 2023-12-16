using Ardalis.SmartEnum;

namespace Interview.Domain.RoomParticipants;

public sealed class RoomParticipantType : SmartEnum<RoomParticipantType>
{
    public static readonly RoomParticipantType Viewer = new("Viewer", EVRoomParticipantType.Viewer);
    public static readonly RoomParticipantType Expert = new("Expert", EVRoomParticipantType.Expert);
    public static readonly RoomParticipantType Examinee = new("Examinee", EVRoomParticipantType.Examinee);

    private RoomParticipantType(string name, EVRoomParticipantType value)
        : base(name, (int)value)
    {
    }

    public EVRoomParticipantType EnumValue => (EVRoomParticipantType)Value;
}
