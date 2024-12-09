using Interview.Domain.Users.Roles;
using NSpecifications;

namespace Interview.Domain.Users.Specification;

public sealed class UserByRoleSpecification(RoleName name) : Spec<User>(e => Enumerable.Any(e.Roles, r => r.Name == name));
