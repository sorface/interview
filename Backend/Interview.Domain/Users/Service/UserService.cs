using System.Collections.Immutable;
using Interview.Domain.Permissions;
using Interview.Domain.Repository;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly AdminUsers _adminUsers;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ISecurityService _securityService;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        AdminUsers adminUsers,
        IPermissionRepository permissionRepository,
        ISecurityService securityService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _adminUsers = adminUsers;
        _permissionRepository = permissionRepository;
        _securityService = securityService;
    }

    public Task<IPagedList<UserDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var mapperUserDetail = new Mapper<User, UserDetail>(user => new UserDetail
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Roles = user.Roles.Select(role => role.Name.Name).ToList(),
            TwitchIdentity = user.TwitchIdentity,
        });

        return _userRepository.GetPageDetailedAsync(mapperUserDetail, pageNumber, pageSize, cancellationToken);
    }

    public async Task<UserDetail> FindByNicknameAsync(
        string nickname, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByNicknameAsync(nickname, cancellationToken);

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
            TwitchIdentity = user.TwitchIdentity,
        };
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(id, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(id);
        }

        return user;
    }

    public Task<UserDetail> GetSelfAsync()
    {
        var user = _securityService.CurrentUser();

        if (user is null)
        {
            throw new AccessDeniedException("User unauthorized");
        }

        return Task.FromResult(new UserDetail()
        {
            Id = user.Id,
            TwitchIdentity = user.TwitchIdentity,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Roles = user.Roles.Select(role => role.Name.Name).ToList(),
            Permissions = user.Permissions.Select(permission => new PermissionDetail(permission.Type)).ToList(),
        });
    }

    public async Task<User> UpsertByTwitchIdentityAsync(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.FindByTwitchIdentityAsync(user.TwitchIdentity, cancellationToken);
        if (existingUser != null)
        {
            existingUser.Nickname = user.Nickname;
            existingUser.Avatar = user.Avatar;

            var permissions = await GetDefaultUserPermission(existingUser, cancellationToken);
            existingUser.Permissions.AddRange(permissions);
            await _userRepository.UpdateAsync(existingUser, cancellationToken);
            return existingUser;
        }

        var userRole = await GetUserRoleAsync(user.Nickname, cancellationToken);

        if (userRole is null)
        {
            throw new NotFoundException(ExceptionMessage.UserRoleNotFound());
        }

        var insertUser = new User(user.Nickname, user.TwitchIdentity) { Avatar = user.Avatar };

        insertUser.Roles.Add(userRole);

        var defaultUserPermissions = await GetDefaultUserPermission(insertUser, cancellationToken);
        insertUser.Permissions.AddRange(defaultUserPermissions);

        await _userRepository.CreateAsync(insertUser, cancellationToken);

        return insertUser;
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
            TwitchIdentity = user.TwitchIdentity,
        });

        return _userRepository.GetPageAsync(spec, mapper, pageNumber, pageSize, cancellationToken);
    }

    public async Task<Dictionary<string, List<PermissionItem>>> GetPermissionsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var specification = new Spec<User>(userItem => userItem.Id == id);

        var user = await _userRepository.FindFirstOrDefaultAsync(specification, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(id);
        }

        return await _userRepository.FindPermissionByUserId(id, cancellationToken);
    }

    public async Task<PermissionItem> ChangePermissionAsync(
        Guid id,
        PermissionModifyRequest permissionModifyRequest,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdDetailedAsync(id, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(id);
        }

        var storagePermission =
            await _permissionRepository.FindByIdAsync(permissionModifyRequest.Id, cancellationToken);

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

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new PermissionItem(storagePermission.Type, containsPermission);
    }

    private Task<Role?> GetUserRoleAsync(string nickname, CancellationToken cancellationToken)
    {
        var roleName = _adminUsers.IsAdmin(nickname) ? RoleName.Admin : RoleName.User;

        return _roleRepository.FindByIdAsync(roleName.Id, cancellationToken);
    }

    private Task<List<Permission>> GetDefaultUserPermission(User user, CancellationToken cancellationToken = default)
    {
        var existsPermissions = user.Permissions?.Select(e => e.Id).ToList() ?? (IReadOnlyCollection<Guid>)Array.Empty<Guid>();
        var permissionTypes = user.Roles
            .SelectMany(it => it.Name.DefaultPermissions)
            .ToHashSet();

        var specification = new Spec<Permission>(e => permissionTypes.Contains(e.Type) && !existsPermissions.Contains(e.Id));
        return _permissionRepository.FindAsync(specification, cancellationToken);
    }
}
