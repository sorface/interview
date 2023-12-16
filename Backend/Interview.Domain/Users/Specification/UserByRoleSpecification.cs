using Interview.Domain.Users.Roles;
using NSpecifications;

namespace Interview.Domain.Users.Specification;

public sealed class UserByRoleSpecification : Spec<User>
{
    public UserByRoleSpecification(RoleName name)
        : base(e => e.Roles.Any(r => r.Name == name))
    {
    }
}
