using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users.Roles;

namespace Interview.Domain.Events.Service.FindPage;

#pragma warning disable SA1402
public class AppEventItem<TParticipantType>
#pragma warning restore SA1402
{
    public required Guid Id { get; set; }

    public required string Type { get; set; }

    public required bool Stateful { get; set; }

    public required ICollection<RoleNameType> Roles { get; set; }

    public required ICollection<TParticipantType> ParticipantTypes { get; set; }
}

#pragma warning disable SA1402
public sealed class AppEventItemParticipantType : AppEventItem<SERoomParticipantType>
#pragma warning restore SA1402
{
    public AppEventItem ToAppEventItem()
    {
        return new AppEventItem
        {
            Id = Id,
            Type = Type,
            Roles = Roles,
            ParticipantTypes = ParticipantTypes.Select(e => e.Name)
                .ToList(),
            Stateful = Stateful,
        };
    }
}

public sealed class AppEventItem : AppEventItem<string>
{
}
