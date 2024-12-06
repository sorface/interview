namespace Interview.Domain.Users.Records;

public class UserDetail
{
    public Guid Id { get; set; }

    public string TwitchIdentity { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public string? Avatar { get; init; } = string.Empty;

    public List<string> Roles { get; init; } = [];

    public List<PermissionDetail> Permissions { get; init; } = [];
}
