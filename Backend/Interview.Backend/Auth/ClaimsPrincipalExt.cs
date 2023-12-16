using System.Security.Claims;
using AspNet.Security.OAuth.Twitch;

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
        var twitchId = self.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier);
        var nickname = self.Claims.FirstOrDefault(e => e.Type == ClaimTypes.Name);

        if (twitchId == null || nickname == null)
        {
            return null;
        }

        var id = self.Claims.FirstOrDefault(e => e.Type == UserClaimConstants.UserId);

        var profileImage =
            self.Claims.FirstOrDefault(e => e.Type == TwitchAuthenticationConstants.Claims.ProfileImageUrl);

        var user = new User(nickname.Value, twitchId.Value);

        if (id != null && Guid.TryParse(id.Value, out var typedId))
        {
            user.Id = typedId;
        }

        user.Avatar = profileImage?.Value;

        return user;
    }
}
