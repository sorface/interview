using System.Security.Claims;
using Interview.Backend.Auth.Sorface;

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
        var profileId = self.Claims.FirstOrDefault(e => e.Type == SorfaceClaimTypes.Claims.Profile.Id);
        var nickname = self.Claims.FirstOrDefault(e => e.Type == SorfaceClaimTypes.Claims.Profile.Username);

        if (profileId == null || nickname == null)
        {
            return null;
        }

        var id = self.Claims.FirstOrDefault(e => e.Type == UserClaimConstants.UserId);

        var profileImage = self.Claims.FirstOrDefault(e => e.Type == SorfaceClaimTypes.Claims.Profile.Avatar);

        var user = new User(nickname.Value, profileId.Value);

        if (id != null && Guid.TryParse(id.Value, out var typedId))
        {
            user.Id = typedId;
        }

        user.Avatar = profileImage?.Value;

        var authoritiesClaim = self.Claims.FirstOrDefault(e => e.Type == SorfaceClaimTypes.Claims.Profile.Authorities);

        if (authoritiesClaim is null)
        {
            return user;
        }

        var values = authoritiesClaim.Value;

        var strings = values.Split(',');

        foreach (var authority in strings)
        {
            var roleName = RoleName.FromName(authority, true);
            user.Roles.Add(new Role(roleName));
        }

        return user;
    }
}
