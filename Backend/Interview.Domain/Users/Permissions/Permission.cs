using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomParticipants;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.Users.Permissions;

public class Permission(SEPermission permission) : Entity(permission.Id)
{
    private Permission()
        : this(SEPermission.Unknown)
    {
    }

    public SEPermission Type { get; set; } = permission;

    public List<RoomParticipant> Participants = new();
}
