using Interview.Domain.Permissions;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;
using Interview.Domain.Users.Service;
using X.PagedList;

namespace Interview.Domain.Users.Permissions;

public class UserServicePermissionAccessor : IUserService, IServiceDecorator
{
    private readonly ISecurityService _securityService;
    private readonly IUserService _userService;

    public UserServicePermissionAccessor(ISecurityService securityService, IUserService userService)
    {
        _securityService = securityService;
        _userService = userService;
    }

    public Task<IPagedList<UserDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.UserFindPage);
        return _userService.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<UserDetail> FindByNicknameAsync(string nickname, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.UserFindByNickname);
        return _userService.FindByNicknameAsync(nickname, cancellationToken);
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.UserFindById);
        return _userService.FindByIdAsync(id, cancellationToken);
    }

    public Task<UserDetail> GetSelfAsync()
    {
        return _userService.GetSelfAsync();
    }

    public Task<User> UpsertByTwitchIdentityAsync(User user, CancellationToken cancellationToken = default)
    {
        // _securityService.EnsurePermission(SEPermission.UserFindById);
        return _userService.UpsertByTwitchIdentityAsync(user, cancellationToken);
    }

    public Task<IPagedList<UserDetail>> FindByRoleAsync(
        int pageNumber, int pageSize, RoleNameType roleNameType, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.UserFindByRole);
        return _userService.FindByRoleAsync(pageNumber, pageSize, roleNameType, cancellationToken);
    }

    public Task<Dictionary<string, List<PermissionItem>>> GetPermissionsAsync(
        Guid id, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.UserGetPermissions);
        return _userService.GetPermissionsAsync(id, cancellationToken);
    }

    public Task<PermissionItem> ChangePermissionAsync(
        Guid id, PermissionModifyRequest permissionModifyRequest, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.UserChangePermission);
        return _userService.ChangePermissionAsync(id, permissionModifyRequest, cancellationToken);
    }
}
