using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;
using X.PagedList;

namespace Interview.Domain.Users.Service;

public interface IUserService : IService
{
    Task<IPagedList<UserDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<UserDetail> FindByNicknameAsync(
        string nickname, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDetail> GetSelfAsync();

    Task<User?> UpsertByExternalIdAsync(User user, CancellationToken cancellationToken = default);

    Task<IPagedList<UserDetail>> FindByRoleAsync(
        int pageNumber,
        int pageSize,
        RoleNameType roleNameType,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, List<PermissionItem>>> GetPermissionsAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PermissionItem> ChangePermissionAsync(
        Guid id,
        PermissionModifyRequest permissionModifyRequest,
        CancellationToken cancellationToken);
}
