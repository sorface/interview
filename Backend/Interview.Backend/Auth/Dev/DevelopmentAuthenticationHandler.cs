using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Interview.Backend.Auth.Sorface;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Users.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Interview.Backend.Auth.Dev;

public class DevelopmentAuthenticationHandler(
    IOptionsMonitor<DevelopmentAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<DevelopmentAuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            Logger.LogDebug("No authorization header found");
            return AuthenticateResult.Fail("Unauthorized");
        }

        var auth = AuthenticationHeaderValue.Parse(authorizationHeader!);

        if (auth.Scheme == "Bearer")
        {
            Logger.LogDebug("Bearer token found");

            // If Bearer is used, it means the user wants to use the REAL authentication method and not the development accounts.
            return AuthenticateResult.Fail("Bearer requests should use the real JWT validation scheme");
        }

        // Dumb workaround for NSwag/Swagger: I can't find a way to make it automatically pass "DevBearer" in the auth header.
        // Having to type DevBearer everytime is annoying. So if it is missing, we just pretend it's there.
        // This means you can either pass "ACCOUNT_NAME" in the Authorization header OR "DevBearer ACCOUNT_NAME".
        if (auth.Parameter == null)
        {
            auth = new AuthenticationHeaderValue("DevBearer", auth.Scheme);
        }

        List<Claim> claims;
        try
        {
            var user = auth.Parameter;
            claims = await GetClaimsForUser(user);
        }
        catch (ArgumentException e)
        {
            return AuthenticateResult.Fail(e);
        }

        var identity = new ClaimsIdentity(claims, "DevBearer");
        var principal = new ClaimsPrincipal(identity);

        // Add extra claims if you want to
        await Options.OnTokenValidated(Context, principal);

        var ticket = new AuthenticationTicket(principal, "DevBearer");

        return AuthenticateResult.Success(ticket);
    }

    private async Task<List<Claim>> GetClaimsForUser(string? user)
    {
        var token = user;
        var roles = GetRolesForUser(user).ToList();
        Logger.LogDebug("Found roles in token [{Roles}]", string.Join(", ", roles));

        user = user?.Split(" ")?.FirstOrDefault();
        if (user is null || string.IsNullOrWhiteSpace(user))
        {
            Logger.LogWarning("Failed to find the user's Nickname in the token. {Token}", token);
            throw new AccessDeniedException("Can't set specific account for local development because the user is not recognized");
        }

        var actualUserRoles = RoleName.List.Join(roles,
            name => name.Name,
            role => role,
            (name, _) => new Role(name)).ToList();
        if (actualUserRoles.Count != roles.Count)
        {
            Logger.LogWarning("A match was not found for every role listed. Specified Roles: [{SpecifiedRoles}], Matched Roles: [{MatchedRoles}]. Make sure the case in the role name matches. Available Roles: [{AvailableRoles]}",
                string.Join(", ", roles),
                string.Join(", ", actualUserRoles.Select(e => e.Name)),
                string.Join(", ", RoleName.List.Select(e => e.Name)));
            throw new AccessDeniedException(string.Format("A match was not found for every role listed. Specified Roles: [{0}], Matched Roles: [{1}]. Make sure the case in the role name matches. Available Roles: [{2}]",
                string.Join(", ", roles),
                string.Join(", ", actualUserRoles.Select(e => e.Name)),
                string.Join(", ", RoleName.List.Select(e => e.Name))));
        }

        var appDbContext = Context.RequestServices.GetRequiredService<AppDbContext>();
        var dbUser = await appDbContext.Users
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.Nickname == user);
        if (dbUser is null)
        {
            Logger.LogDebug("No user found by {Nickname} in database, a new user must be created", user);
            dbUser = new User(user, Guid.NewGuid().ToString("N"));

            foreach (var role in actualUserRoles)
            {
                dbUser.Roles.Add(role);
            }

            if (dbUser.Roles.Count == 0)
            {
                Logger.LogWarning("User should have at least one role");
                throw new AccessDeniedException("User should have at least one role. User format: 'DevBearer USER_NICKNAME ROLE1_ROLE2'");
            }
        }
        else
        {
            if (actualUserRoles.Count > 0)
            {
                Logger.LogDebug("Update user roles");
                dbUser.Roles.Clear();
                foreach (var role in actualUserRoles)
                {
                    dbUser.Roles.Add(role);
                }
            }
            else
            {
                Logger.LogDebug("No roles to update db user");
            }
        }

        Logger.LogDebug("Upsert user in database");
        var userService = Context.RequestServices.GetRequiredService<IUserService>();
        dbUser = await userService.UpsertByExternalIdAsync(dbUser);

        var res = new List<Claim>
        {
            new(SorfaceClaimTypes.Claims.Profile.Id, dbUser.ExternalId, ClaimValueTypes.String),
            new(SorfaceClaimTypes.Claims.Profile.Username, dbUser.Nickname, ClaimValueTypes.String),
        };

        if (dbUser.Avatar is not null)
        {
            res.Add(new(SorfaceClaimTypes.Claims.Profile.Avatar, dbUser.Avatar, ClaimValueTypes.String));
        }

        if (dbUser.Id != Guid.Empty)
        {
            res.Add(new(UserClaimConstants.UserId, dbUser.Id.ToString(), ClaimValueTypes.String));
        }

        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        var updatedRoles = roles.Count == 0 ? dbUser.Roles.Select(e => e.Name.Name) : roles;
        foreach (var role in updatedRoles)
        {
            // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
            res.Add(new(SorfaceClaimTypes.Claims.Profile.Authorities, role, ClaimValueTypes.String));
        }

        return res;

        IEnumerable<string> GetRolesForUser(string? userToken)
        {
            if (string.IsNullOrWhiteSpace(userToken))
            {
                yield break;
            }

            var tokenParts = userToken.Split(" ");
            if (tokenParts.Length <= 1)
            {
                yield break;
            }

            foreach (var role in tokenParts[1].Split('_', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                yield return role;
            }
        }
    }
}
