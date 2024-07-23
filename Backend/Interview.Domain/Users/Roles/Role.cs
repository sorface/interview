using Interview.Domain.Repository;

namespace Interview.Domain.Users.Roles;

public class Role : Entity
{
    public Role(RoleName name)
        : base(name.Id)
    {
        Name = name;
    }

    private Role()
        : this(RoleName.Unknown)
    {
    }

    public RoleName Name { get; private set; }
}
