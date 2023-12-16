using Interview.Domain.Repository;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Roles;

namespace Interview.Domain.Users;

public class User : Entity
{
    public User(Guid id, string nickname, string twitchIdentity)
        : base(id)
    {
        Nickname = nickname;
        TwitchIdentity = twitchIdentity;
    }

    public User(string nickname, string twitchIdentity)
        : this(Guid.Empty, nickname, twitchIdentity)
    {
    }

    private User()
        : this(string.Empty, string.Empty)
    {
    }

    public string Nickname { get; internal set; }

    public string? Avatar { get; set; }

    public string TwitchIdentity { get; private set; }

    public List<Role> Roles { get; private set; } = new List<Role>();

    public List<Permission> Permissions { get; private set; } = new List<Permission>();
}
