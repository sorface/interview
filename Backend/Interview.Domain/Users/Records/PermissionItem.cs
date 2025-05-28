using Interview.Domain.Permissions;

namespace Interview.Domain.Users.Records;

public class PermissionItem(Guid id, string type, string description, bool activate)
{
    public PermissionItem(SEPermission permission, bool activate) : this(permission.Id, permission.Name, permission.Description, activate)
    {
    }

    public Guid Id { get; set; } = id;

    public string Type { get; set; } = type;

    public string Description { get; set; } = description;

    public bool Activate { get; set; } = activate;
}
