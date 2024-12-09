using Interview.Domain.Repository;

namespace Interview.Domain.Users.Roles;

public class Role(RoleName name) : Entity(name.Id)
{
    private Role()
        : this(RoleName.Unknown)
    {
    }

    public RoleName Name { get; private set; } = name;
}
