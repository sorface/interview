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

public class SecurityService : ISecurityService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    private readonly ICurrentPermissionAccessor _currentPermissionAccessor;

    private readonly IRoomParticipantRepository _roomParticipantRepository;

    public SecurityService(
        ICurrentPermissionAccessor currentPermissionAccessor,
        ICurrentUserAccessor currentUserAccessor,
        IRoomParticipantRepository roomParticipantRepository)
    {
        _currentPermissionAccessor = currentPermissionAccessor;
        _currentUserAccessor = currentUserAccessor;
        _roomParticipantRepository = roomParticipantRepository;
    }

    public async Task EnsureRoomPermissionAsync(Guid roomId, SEPermission action, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.UserId;
        if (userId is null)
        {
            throw AccessDeniedException.CreateForAction(action.Name);
        }

        var participantPermission = await _roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(roomId, userId.Value, cancellationToken);

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
        if (_currentUserAccessor.IsAdmin())
        {
            return Task.CompletedTask;
        }

        var isProtectedResource = _currentPermissionAccessor.IsProtectedResource(action.Name);

        if (isProtectedResource && _currentUserAccessor.HasPermission(action.Name) is false)
        {
            throw AccessDeniedException.CreateForAction(action.Name);
        }

        return Task.CompletedTask;
    }

    public User? CurrentUser() => _currentUserAccessor.UserDetailed;

    public Guid? CurrentUserId() => _currentUserAccessor.UserId;

    public bool IsAdmin() => _currentUserAccessor.IsAdmin();

    public bool HasRole(RoleName roleName) => _currentUserAccessor.HasRole(roleName);

    public bool HasPermission(SEPermission permission) => _currentUserAccessor.HasPermission(permission.Name);
}
