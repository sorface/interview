using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Interview.Backend.Auth.Sorface;

public class SorfacePrincipalValidator
{
    private const string AccessTokenName = "access_token";
    private const string ExpirationTokenName = "expires_at";

    private readonly ILogger<SorfacePrincipalValidator> _logger;
    private readonly SorfaceTokenHandler _sorfaceTokenHandler;

    public SorfacePrincipalValidator(
        ILogger<SorfacePrincipalValidator> logger,
        SorfaceTokenHandler sorfaceTokenHandler)
    {
        _logger = logger;
        _sorfaceTokenHandler = sorfaceTokenHandler;
    }

    public async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            return;
        }

        try
        {
            var accessToken = context.Properties.GetTokenValue("access_token");

            if (accessToken is null)
            {
                context.RejectPrincipal();

                return;
            }

            using var tokenPrincipal =
                await _sorfaceTokenHandler.GetTokenPrincipalAsync(accessToken, context.HttpContext.RequestAborted);

            if (!tokenPrincipal.RootElement.TryGetProperty("active", out var active))
            {
                context.RejectPrincipal();
                return;
            }

            if (!active.GetBoolean())
            {
                _logger.LogInformation(@"Token refresh operation has been with result {result}", tokenPrincipal);
                context.RejectPrincipal();
            }
        }
        catch (Exception e)
        {
            context.RejectPrincipal();
            _logger.LogError(e, "during token validation");
        }
    }
}
