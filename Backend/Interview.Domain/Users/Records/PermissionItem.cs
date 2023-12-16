using Interview.Domain.Permissions;

namespace Interview.Domain.Users.Records;

public class PermissionItem
{
    public PermissionItem(Guid id, string type, string description, bool activate)
    {
        Id = id;
        Type = type;
        Activate = activate;
        Description = description;
    }

    public PermissionItem(SEPermission permission, bool activate)
    {
        Id = permission.Id;
        Type = permission.Name;
        Activate = activate;
        Description = permission.Description;
    }

    public Guid Id { get; set; }

    public string Type { get; set; }

    public string Description { get; set; }

    public bool Activate { get; set; }
}
