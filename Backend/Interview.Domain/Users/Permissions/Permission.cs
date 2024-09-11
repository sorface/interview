using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomParticipants;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.Users.Permissions;

public class Permission : Entity
{
    public Permission(SEPermission permission)
        : base(permission.Id)
    {
        Type = permission;
    }

    private Permission()
        : this(SEPermission.Unknown)
    {
    }

    public SEPermission Type { get; set; }

    public List<RoomParticipant> Participants = new();
}
