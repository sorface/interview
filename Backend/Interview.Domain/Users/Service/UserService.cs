using Interview.Backend.Auth;
using Interview.Domain.Database;
using Interview.Domain.Permissions;
using Interview.Domain.Repository;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;
using Microsoft.Extensions.Caching.Memory;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Users.Service;

public sealed class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    ISecurityService securityService,
    AppDbContext databaseContext,
    IMemoryCache memoryCache,
    SemaphoreLockProvider<string> externalUserIdLockProvider)
    : IUserService
{
    public Task<IPagedList<UserDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var mapperUserDetail = new Mapper<User, UserDetail>(user => new UserDetail
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Roles = user.Roles.Select(role => role.Name.Name).ToList(),
            TwitchIdentity = user.ExternalId,
        });

        return userRepository.GetPageDetailedAsync(mapperUserDetail, pageNumber, pageSize, cancellationToken);
    }

    public async Task<UserDetail> FindByNicknameAsync(
        string nickname, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByNicknameAsync(nickname, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(ExceptionMessage.UserNotFoundByNickname(nickname));
        }

        return new UserDetail
        {
            Id = user.Id,
            Avatar = user.Avatar,
            Nickname = user.Nickname,
            Roles = user.Roles.Select(role => role.Name.Name).ToList(),
            TwitchIdentity = user.ExternalId,
        };
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(id, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(id);
        }

        return user;
    }

    public Task<UserDetail> GetSelfAsync()
    {
        var user = securityService.CurrentUser();

        if (user is null)
        {
            throw new AccessDeniedException("User unauthorized");
        }

        return Task.FromResult(new UserDetail()
        {
            Id = user.Id,
            TwitchIdentity = user.ExternalId,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Roles = user.Roles.Select(role => role.Name.Name).ToList(),
            Permissions = user.Permissions.Select(permission => new PermissionDetail(permission.Type)).ToList(),
        });
    }

    public async Task<User?> UpsertByExternalIdAsync(User user, CancellationToken cancellationToken = default)
    {
        var roles = user.Roles.Select(role => role.Name.Name).Order();

        var externalIdLock = externalUserIdLockProvider.CreateWaiter("User:" + user.ExternalId);
        var cachedUser = new { user.Nickname, user.Avatar, user.ExternalId, Roles = string.Join(",", roles) };

        // пользователи массово создаются из-за того, что в кэш запись не будет добавлена
        return await memoryCache.GetOrCreateAsync<User>(cachedUser, async entry =>
        {
            using (externalIdLock)
            {
                await externalIdLock.WaitAsync(cancellationToken);

                entry.SlidingExpiration = TimeSpan.FromMinutes(3);

                return await databaseContext.RunTransactionAsync(async token =>
                {
                    var existingUser = await userRepository.FindByExternalIdAsync(user.ExternalId, token);

                    var roles = user.Roles.DefaultIfEmpty(new Role(RoleName.User));

                    var userRoles = await GetUserRolesAsync(roles, token);

                    if (userRoles is null || !userRoles.Any())
                    {
                        throw new NotFoundException(ExceptionMessage.UserRoleNotFound());
                    }

                    if (existingUser != null)
                    {
                        existingUser.Nickname = user.Nickname;
                        existingUser.Avatar = user.Avatar;

                        existingUser.Roles.Clear();
                        existingUser.Roles.AddRange(userRoles);

                        var permissions = await GetDefaultUserPermission(existingUser, token);
                        existingUser.Permissions.AddRange(permissions);

                        await userRepository.UpdateAsync(existingUser, token);

                        return existingUser;
                    }

                    var insertUser = new User(user.Nickname, user.ExternalId) { Avatar = user.Avatar };

                    insertUser.Roles.Clear();
                    insertUser.Roles.AddRange(userRoles);

                    var defaultUserPermissions = await GetDefaultUserPermission(insertUser, token);
                    insertUser.Permissions.AddRange(defaultUserPermissions);

                    await userRepository.CreateAsync(insertUser, token);

                    return insertUser;
                }, cancellationToken);
            }
        });
    }

    public Task<IPagedList<UserDetail>> FindByRoleAsync(
        int pageNumber,
        int pageSize,
        RoleNameType roleNameType,
        CancellationToken cancellationToken = default)
    {
        var roleName = RoleName.FromValue((int)roleNameType);

        var spec = new Spec<User>(user => user.Roles.Any(r => r.Name == roleName));
        var mapper = new Mapper<User, UserDetail>(user => new UserDetail
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Roles = user.Roles.Select(role => role.Name.Name).ToList(),
            TwitchIdentity = user.ExternalId,
        });

        return userRepository.GetPageAsync(spec, mapper, pageNumber, pageSize, cancellationToken);
    }

    public async Task<Dictionary<string, List<PermissionItem>>> GetPermissionsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var specification = new Spec<User>(userItem => userItem.Id == id);

        var user = await userRepository.FindFirstOrDefaultAsync(specification, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(id);
        }

        return await userRepository.FindPermissionByUserId(id, cancellationToken);
    }

    public async Task<PermissionItem> ChangePermissionAsync(
        Guid id,
        PermissionModifyRequest permissionModifyRequest,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdDetailedAsync(id, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(id);
        }

        var storagePermission =
            await permissionRepository.FindByIdAsync(permissionModifyRequest.Id, cancellationToken);

        if (storagePermission is null)
        {
            throw NotFoundException.Create<Permission>(id);
        }

        var permission = user.Permissions.FirstOrDefault(permission => permission.Id == permissionModifyRequest.Id);

        if (permission is not null && permissionModifyRequest.Activate)
        {
            throw new UserException("User already has this permission");
        }

        if (permission is null && permissionModifyRequest.Activate is false)
        {
            throw new UserException("The user's permission is already disabled");
        }

        var containsPermission = false;

        if (permissionModifyRequest.Activate && permission is null)
        {
            user.Permissions.Add(storagePermission);
            containsPermission = true;
        }
        else if (permissionModifyRequest.Activate is not true && permission is not null)
        {
            user.Permissions.Remove(storagePermission);
            containsPermission = false;
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return new PermissionItem(storagePermission.Type, containsPermission);
    }

    private Task<List<Role>> GetUserRolesAsync(IEnumerable<Role> roles, CancellationToken cancellationToken)
    {
        var roleNames = roles.Select(r => r.Name);
        return roleRepository.FindAsync(new Spec<Role>(role => roleNames.Contains(role.Name)), cancellationToken);
    }

    private Task<List<Permission>> GetDefaultUserPermission(User user, CancellationToken cancellationToken = default)
    {
        var existsPermissions = user.Permissions.Select(e => e.Id).ToList() ?? (IReadOnlyCollection<Guid>)Array.Empty<Guid>();
        var permissionTypes = user.Roles
            .SelectMany(it => it.Name.DefaultPermissions)
            .ToHashSet();

        var specification = new Spec<Permission>(e => permissionTypes.Contains(e.Type) && !existsPermissions.Contains(e.Id));
        return permissionRepository.FindAsync(specification, cancellationToken);
    }
}
