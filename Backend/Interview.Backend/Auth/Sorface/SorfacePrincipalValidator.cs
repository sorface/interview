using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Interview.Backend.Auth.Sorface;

public class SorfacePrincipalValidator
{
    private readonly ILogger<SorfacePrincipalValidator> _logger;
    private readonly SorfaceTokenService _sorfaceTokenService;

    public SorfacePrincipalValidator(ILogger<SorfacePrincipalValidator> logger, SorfaceTokenService sorfaceTokenService)
    {
        _logger = logger;
        _sorfaceTokenService = sorfaceTokenService;
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
            var accessToken = context.Properties.GetTokenValue(SorfaceTokenDefaults.AccessTokenName);

            if (accessToken is null)
            {
                context.RejectPrincipal();

                return;
            }

            using var tokenPrincipal =
                await _sorfaceTokenService.GetTokenPrincipalAsync(accessToken, context.HttpContext.RequestAborted);

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
