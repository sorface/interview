using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Interview.Backend.Auth.Sorface;

public class SorfacePrincipalValidator
{
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var accessToken = context.Properties.GetTokenValue("access_token");

        if (accessToken is null)
        {
            context.RejectPrincipal();

            return;
        }

        try
        {
            var requiredService = context.HttpContext.RequestServices.GetRequiredService<SorfaceTokenValidateHandler>();

            using var tokenPrincipal = await requiredService.GetTokenPrincipalAsync(accessToken, context.HttpContext.RequestAborted);

            if (!tokenPrincipal.RootElement.TryGetProperty("active", out var active))
            {
                context.RejectPrincipal();
                return;
            }

            if (!active.GetBoolean())
            {
                context.RejectPrincipal();
            }
        }
        catch (Exception e)
        {
            context.RejectPrincipal();

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SorfacePrincipalValidator>>();

            logger.LogError(e, "during token validation");
        }
    }
}
