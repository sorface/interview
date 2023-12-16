using Interview.Domain.Permissions;
using Interview.Domain.Users.Permissions;

namespace Interview.Domain.Users.Records;

public class PermissionDetail
{
    public Guid Id { get; init; }

    public string Code { get; init; }

    public string Description { get; init; }

    public PermissionDetail(SEPermission permission)
    {
        Id = permission.Id;
        Code = permission.Name;
        Description = permission.Description;
    }

    public PermissionDetail()
    {
    }
}
