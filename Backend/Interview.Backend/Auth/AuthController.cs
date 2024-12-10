using Interview.Backend.Auth.Sorface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.Auth;

[ApiController]
[Route("api")]
public class AuthController(
    OAuthServiceDispatcher oAuthDispatcher,
    ILogger<AuthController> logger,
    SorfaceTokenService sorfaceTokenService)
    : ControllerBase
{
    [HttpGet("login/{scheme}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(string), 400)]
    public IResult SignIn(string scheme, [FromQuery] string redirectUri)
    {
        logger.LogInformation("Start SignIn {scheme}", scheme);
        if (!oAuthDispatcher.HasAuthService(scheme))
        {
            logger.LogInformation("Not found service authorization with id {scheme}", scheme);
            return Results.BadRequest($"Not found service authorization with id ${scheme}");
        }

        var redirectUriWithoutFragment = GetDomain(redirectUri);
        var authorizationService = oAuthDispatcher.GetAuthService(scheme);
        logger.LogInformation("Get {AuthService} by {scheme}", authorizationService?.GetType().Name, scheme);
        if (authorizationService != null && !authorizationService.AvailableLoginRedirects.Contains(redirectUriWithoutFragment))
        {
            logger.LogWarning("Redirect link {redirectUri} is not available", redirectUri);
            return Results.BadRequest($"Redirect link {redirectUri} is not available");
        }

        var authenticationProperties = new AuthenticationProperties { RedirectUri = redirectUri, };

        logger.LogDebug("Before change");
        var signIn = Results.Challenge(authenticationProperties, authenticationSchemes: new List<string> { scheme });
        logger.LogDebug("After change");
        return signIn;

        static string GetDomain(string uri)
        {
            try
            {
                var typedUri = new Uri(uri);
                return typedUri.Scheme + "://" + typedUri.Authority;
            }
            catch
            {
                return uri;
            }
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task SignOutImpl([FromServices] ICurrentUserAccessor currentUserAccessor)
    {
        currentUserAccessor.GetUserIdOrThrow();

        var accessToken = await HttpContext.GetTokenAsync(SorfaceTokenDefaults.AccessTokenName);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties());

        await sorfaceTokenService.RevokeTokenAsync(HttpContext, accessToken);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(string), 400)]
    public Task<ActionResult> RefreshPrincipalToken()
    {
        return Task.FromResult<ActionResult>(Ok());
    }
}
