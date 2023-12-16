using Interview.Domain.Users;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;

namespace Interview.Domain.Permissions;

public interface ISecurityService : IService
{
    public void EnsurePermission(SEPermission action);

    public User? CurrentUser();

    public Guid? CurrentUserId();

    public bool IsAdmin();

    public bool HasRole(RoleName roleName);

    public bool HasPermission(SEPermission permission);
}

public class SecurityService : ISecurityService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    private readonly ICurrentPermissionAccessor _currentPermissionAccessor;

    public SecurityService(ICurrentPermissionAccessor currentPermissionAccessor, ICurrentUserAccessor currentUserAccessor)
    {
        _currentPermissionAccessor = currentPermissionAccessor;
        _currentUserAccessor = currentUserAccessor;
    }

    public void EnsurePermission(SEPermission action)
    {
        if (_currentUserAccessor.IsAdmin())
        {
            return;
        }

        var isProtectedResource = _currentPermissionAccessor.IsProtectedResource(action.Name);

        if (isProtectedResource && _currentUserAccessor.HasPermission(action.Name) is false)
        {
            throw AccessDeniedException.CreateForAction(action.Name);
        }
    }

    public User? CurrentUser() => _currentUserAccessor.UserDetailed;

    public Guid? CurrentUserId() => _currentUserAccessor.UserId;

    public bool IsAdmin() => _currentUserAccessor.IsAdmin();

    public bool HasRole(RoleName roleName) => _currentUserAccessor.HasRole(roleName);

    public bool HasPermission(SEPermission permission) => _currentUserAccessor.HasPermission(permission.Name);
}
