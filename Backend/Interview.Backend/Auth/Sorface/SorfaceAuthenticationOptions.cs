using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Interview.Backend.Auth.Sorface;

public class SorfaceAuthenticationOptions : OAuthOptions
{
    public SorfaceAuthenticationOptions()
    {
        ClaimActions.MapCustomJson(ClaimTypes.NameIdentifier, user => GetData(user, "id"));
        ClaimActions.MapCustomJson(ClaimTypes.Name, user => GetData(user, "username"));
        ClaimActions.MapCustomJson(ClaimTypes.Email, user => GetData(user, "email"));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the "force_verify=true" flag should be sent to Twitch.
    /// When set to <c>true</c>, Twitch displays the consent screen for every authorization request.
    /// When left to <c>false</c>, the consent screen is skipped if the user is already logged in.
    /// </summary>
    public bool ForceVerify { get; set; }

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
