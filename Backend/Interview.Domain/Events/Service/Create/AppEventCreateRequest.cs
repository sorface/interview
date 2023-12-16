using Interview.Domain.Users.Roles;

namespace Interview.Domain.Events.Service.Create;

public class AppEventCreateRequest
{
    public required string Type { get; set; }

    public required bool Stateful { get; set; }

    public required ICollection<RoleNameType> Roles { get; set; }

    public required IReadOnlyCollection<string> ParticipantTypes { get; set; }
}
