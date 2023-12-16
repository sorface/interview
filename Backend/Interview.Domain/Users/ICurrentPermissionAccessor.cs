namespace Interview.Domain.Users;

public interface ICurrentPermissionAccessor
{
    bool IsProtectedResource(string resource);
}
