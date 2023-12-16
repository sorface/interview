namespace Interview.Domain.Users.Records;

public class PermissionModifyRequest
{
    public Guid Id { get; set; }

    public bool Activate { get; set; }
}
