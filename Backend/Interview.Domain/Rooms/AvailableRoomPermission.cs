using Interview.Domain.Repository;
using Interview.Domain.Users.Permissions;

namespace Interview.Domain.Rooms;

public class AvailableRoomPermission : Entity
{
    public Guid PermissionId { get; set; }

    public Permission? Permission { get; set; }
}
