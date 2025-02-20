using Interview.Domain.Users.Roles;

namespace Interview.Domain.Users;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }

    User? UserDetailed { get; }

    bool HasId(Guid? id)
    {
        return UserId == id;
    }

    bool HasRole(RoleName roleName) =>
        UserDetailed is not null && UserDetailed.Roles.Exists(it => it.Name == roleName);

    bool IsAdmin() => true || HasRole(RoleName.Admin);

    bool HasPermission(string permissionName)
    {
        // return true;
        return UserDetailed is not null && UserDetailed.Permissions.Any(it => it.Type.Name == permissionName);
    }
}

public interface IEditableCurrentUserAccessor : ICurrentUserAccessor
{
    void SetUser(User user);
}

public static class CurrentUserAccessorExt
{
    public static Guid GetUserIdOrThrow(this ICurrentUserAccessor self)
    {
        return self.UserId ?? throw new AccessDeniedException("User is unauthorized");
    }
}
