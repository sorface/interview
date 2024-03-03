using System.Collections.Immutable;
using Ardalis.SmartEnum;
using Interview.Domain.Permissions;

namespace Interview.Domain.Rooms.RoomParticipants;

public sealed class SERoomParticipantType : SmartEnum<SERoomParticipantType>
{
    public static readonly SERoomParticipantType Viewer = new("Viewer", EVRoomParticipantType.Viewer, ImmutableHashSet<SEPermission>.Empty);
    public static readonly SERoomParticipantType Expert = new("Expert", EVRoomParticipantType.Expert, ImmutableHashSet<SEPermission>.Empty);
    public static readonly SERoomParticipantType Examinee = new("Examinee", EVRoomParticipantType.Examinee, ImmutableHashSet<SEPermission>.Empty);

    private SERoomParticipantType(string name, EVRoomParticipantType value, IReadOnlySet<SEPermission> defaultRoomPermission)
        : base(name, (int)value)
    {
        DefaultRoomPermission = defaultRoomPermission;
    }

    public EVRoomParticipantType EnumValue => (EVRoomParticipantType)Value;

    public IReadOnlySet<SEPermission> DefaultRoomPermission { get; }
}
