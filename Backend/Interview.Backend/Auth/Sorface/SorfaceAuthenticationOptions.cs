using System.Collections;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Interview.Backend.Auth.Sorface;

public class SorfaceAuthenticationOptions : OAuthOptions
{
    public SorfaceAuthenticationOptions()
    {
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Id, user => GetValue(user, "id"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Username, user => GetValue(user, "username"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Email, user => GetValue(user, "email"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Avatar, user => GetValue(user, "avatarUrl"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.FirstName, user => GetValue(user, "firstName"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.LastName, user => GetValue(user, "lastName"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.MiddleName, user => GetValue(user, "middleName"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Authorities, user =>
            string.Join(",", GetArrayString(user, "authorities").ToArray()));
    }

    private static string? GetValue(JsonElement user, string key)
    {
        if (!user.TryGetProperty("principal", out var principalElement))
        {
            return null;
        }

        return !principalElement.TryGetProperty(key, out var propertyValue) ? null : propertyValue.GetString();
    }

    private static IEnumerable<string> GetArrayString(JsonElement user, string key)
    {
        return !user.TryGetProperty("principal", out var principalElement) ? new List<string>() : principalElement.TryGetStringArray(key);
    }
}
