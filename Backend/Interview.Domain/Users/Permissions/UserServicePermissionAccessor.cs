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

    public async Task<IPagedList<UserDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.UserFindPage, cancellationToken);
        return await _userService.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<UserDetail> FindByNicknameAsync(string nickname, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.UserFindByNickname, cancellationToken);
        return await _userService.FindByNicknameAsync(nickname, cancellationToken);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.UserFindById, cancellationToken);
        return await _userService.FindByIdAsync(id, cancellationToken);
    }

    public async Task<UserDetail> GetSelfAsync()
    {
        return await _userService.GetSelfAsync();
    }

    public Task<User> UpsertByExternalIdAsync(User user, CancellationToken cancellationToken = default)
    {
        // await _securityService.EnsurePermission(SEPermission.UserFindById);
        return _userService.UpsertByExternalIdAsync(user, cancellationToken);
    }

    public async Task<IPagedList<UserDetail>> FindByRoleAsync(
        int pageNumber, int pageSize, RoleNameType roleNameType, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.UserFindByRole, cancellationToken);
        return await _userService.FindByRoleAsync(pageNumber, pageSize, roleNameType, cancellationToken);
    }

    public async Task<Dictionary<string, List<PermissionItem>>> GetPermissionsAsync(
        Guid id, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.UserGetPermissions, cancellationToken);
        return await _userService.GetPermissionsAsync(id, cancellationToken);
    }

    public async Task<PermissionItem> ChangePermissionAsync(
        Guid id, PermissionModifyRequest permissionModifyRequest, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.UserChangePermission, cancellationToken);
        return await _userService.ChangePermissionAsync(id, permissionModifyRequest, cancellationToken);
    }
}
