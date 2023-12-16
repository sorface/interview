namespace Interview.Backend.AppEvents;

public class InitialEvent
{
    public required bool Stateful { get; init; }

    public required string Type { get; init; }

    public required RoleNameType[] Roles { get; init; }

    public required HashSet<string> ParticipantTypes { get; init; }
}
