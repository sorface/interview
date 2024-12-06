using Interview.Domain.Permissions;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;
using Interview.Domain.Users.Service;
using X.PagedList;

namespace Interview.Domain.Users.Permissions;

public class UserServicePermissionAccessor(ISecurityService securityService, IUserService userService) : IUserService, IServiceDecorator
{
    public async Task<IPagedList<UserDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.UserFindPage, cancellationToken);
        return await userService.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<UserDetail> FindByNicknameAsync(string nickname, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.UserFindByNickname, cancellationToken);
        return await userService.FindByNicknameAsync(nickname, cancellationToken);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.UserFindById, cancellationToken);
        return await userService.FindByIdAsync(id, cancellationToken);
    }

    public async Task<UserDetail> GetSelfAsync()
    {
        return await userService.GetSelfAsync();
    }

    public Task<User> UpsertByExternalIdAsync(User user, CancellationToken cancellationToken = default)
    {
        // await _securityService.EnsurePermission(SEPermission.UserFindById);
        return userService.UpsertByExternalIdAsync(user, cancellationToken);
    }

    public async Task<IPagedList<UserDetail>> FindByRoleAsync(
        int pageNumber, int pageSize, RoleNameType roleNameType, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.UserFindByRole, cancellationToken);
        return await userService.FindByRoleAsync(pageNumber, pageSize, roleNameType, cancellationToken);
    }

    public async Task<Dictionary<string, List<PermissionItem>>> GetPermissionsAsync(
        Guid id, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.UserGetPermissions, cancellationToken);
        return await userService.GetPermissionsAsync(id, cancellationToken);
    }

    public async Task<PermissionItem> ChangePermissionAsync(
        Guid id, PermissionModifyRequest permissionModifyRequest, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.UserChangePermission, cancellationToken);
        return await userService.ChangePermissionAsync(id, permissionModifyRequest, cancellationToken);
    }
}
