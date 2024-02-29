using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users.Roles;

namespace Interview.Domain.Events;

public class AppEvent : Entity
{
    public required string Type { get; set; }

    public required List<Role>? Roles { get; set; }

    public required List<RoomParticipantType>? ParticipantTypes { get; set; }

    public required bool Stateful { get; set; }

    public static List<RoomParticipantType> ParseParticipantTypes(
        string eventType,
        IReadOnlyCollection<string> participantTypesStr)
    {
        var participantTypes = new List<RoomParticipantType>(participantTypesStr.Count);
        foreach (var participantName in participantTypesStr)
        {
            if (!RoomParticipantType.TryFromName(participantName, out var participantType))
            {
                throw new Exception($"Unknown participant type '{participantName}' for event '{eventType}'");
            }

            participantTypes.Add(participantType);
        }

        return participantTypes;
    }
}
