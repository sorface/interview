using System.Security.Claims;

namespace Interview.Backend.Auth;

public static class ClaimsPrincipalExt
{
    public static void EnrichRolesWithId(this ClaimsPrincipal self, User user)
    {
        var newRoles = user.Roles.Select(e => new Claim(ClaimTypes.Role, e.Name.Name));

        var claimIdentity = new ClaimsIdentity(newRoles);

        claimIdentity.AddClaim(new Claim(UserClaimConstants.UserId, user.Id.ToString()));

        self.AddIdentity(claimIdentity);
    }

    public static User? ToUser(this ClaimsPrincipal self)
    {
        var profileId = self.Claims.FirstOrDefault(e => e.Type == JwtClaimsConstants.ProfileId);
        var nickname = self.Claims.FirstOrDefault(e => e.Type == JwtClaimsConstants.Username);

        if (profileId is null || nickname is null)
        {
            return null;
        }

        var id = self.Claims.FirstOrDefault(e => e.Type == UserClaimConstants.UserId);

        var user = new User(nickname.Value, profileId.Value);

        if (id is not null && Guid.TryParse(id.Value, out var typedId))
        {
            user.Id = typedId;
        }

        var authoritiesClaim = self.Claims.Where(e => e.Type == JwtClaimsConstants.Authorities).Select(it => it.Value);

        foreach (var authority in authoritiesClaim)
        {
            var roleName = RoleName.FromName(authority.Replace("ROLE_", string.Empty), true);
            user.Roles.Add(new Role(roleName));
        }

        return user;
    }
}
