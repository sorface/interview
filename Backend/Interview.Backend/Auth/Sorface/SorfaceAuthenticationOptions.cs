using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Interview.Backend.Auth.Sorface;

public class SorfaceAuthenticationOptions : OAuthOptions
{
    public SorfaceAuthenticationOptions()
    {
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Id, user => GetData(user, "id"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Username, user => GetData(user, "username"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Email, user => GetData(user, "email"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.Avatar, user => GetData(user, "avatarUrl"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.FirstName, user => GetData(user, "firstName"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.LastName, user => GetData(user, "lastName"));
        ClaimActions.MapCustomJson(SorfaceClaimTypes.Claims.Profile.MiddleName, user => GetData(user, "middleName"));
    }

    private static string? GetData(JsonElement user, string key)
    {
        if (!user.TryGetProperty("principal", out var principalElement))
        {
            return null;
        }

        if (!principalElement.TryGetProperty(key, out var propertyValue))
        {
            return null;
        }

        return propertyValue.GetString();
    }
}
