using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users;
using Interview.Domain.Users.Roles;

namespace Interview.Domain.Permissions;

public interface ISecurityService : IService
{
    public Task EnsureRoomPermissionAsync(Guid? roomId, SEPermission action, CancellationToken cancellationToken)
    {
        return roomId is null
            ? EnsurePermissionAsync(action, cancellationToken)
            : EnsureRoomPermissionAsync(roomId.Value, action, cancellationToken);
    }

    Task EnsureRoomPermissionAsync(Guid roomId, SEPermission action, CancellationToken cancellationToken);

    Task EnsurePermissionAsync(SEPermission action, CancellationToken cancellationToken);

    User? CurrentUser();

    Guid? CurrentUserId();

    bool IsAdmin();

    bool HasRole(RoleName roleName);

    bool HasPermission(SEPermission permission);
}

public class SecurityService(
    ICurrentPermissionAccessor currentPermissionAccessor,
    ICurrentUserAccessor currentUserAccessor,
    IRoomParticipantRepository roomParticipantRepository)
    : ISecurityService
{
    public async Task EnsureRoomPermissionAsync(Guid roomId, SEPermission action, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            throw AccessDeniedException.CreateForAction(action.Name);
        }

        var participantPermission = await roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(roomId, userId.Value, cancellationToken);

        // The user may not yet be a member of the room.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (participantPermission?.Permissions != null && participantPermission.Permissions.Any(e => e.Id == action.Id))
        {
            return;
        }

        await EnsurePermissionAsync(action, cancellationToken);
    }

    public Task EnsurePermissionAsync(SEPermission action, CancellationToken cancellationToken)
    {
        if (currentUserAccessor.IsAdmin())
        {
            return Task.CompletedTask;
        }

        var isProtectedResource = currentPermissionAccessor.IsProtectedResource(action.Name);

        if (isProtectedResource && currentUserAccessor.HasPermission(action.Name) is false)
        {
            throw AccessDeniedException.CreateForAction(action.Name);
        }

        return Task.CompletedTask;
    }

    public User? CurrentUser() => currentUserAccessor.UserDetailed;

    public Guid? CurrentUserId() => currentUserAccessor.UserId;

    public bool IsAdmin() => currentUserAccessor.IsAdmin();

    public bool HasRole(RoleName roleName) => currentUserAccessor.HasRole(roleName);

    public bool HasPermission(SEPermission permission) => currentUserAccessor.HasPermission(permission.Name);
}
