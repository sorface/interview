using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Interview.Backend.Auth.Sorface;

public class SorfacePrincipalValidator
{
    private const string AccessTokenName = "access_token";
    private const string RefreshTokenName = "refresh_token";
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

    public async Task<Result> RefreshTokenAsync(HttpContext context)
    {
        var exp = await context.GetTokenAsync(ExpirationTokenName);

        if (exp is null)
        {
            _logger.Log(LogLevel.Information, "Exp is null. not refreshed");
            return Result.None;
        }

        _logger.LogInformation(@"get exp {exp}", exp);

        var refreshToken = await context.GetTokenAsync(RefreshTokenName);

        if (refreshToken is null)
        {
            _logger.Log(LogLevel.Warning, "refreshToken is null");
            return Result.RejectPrincipal;
        }

        RefreshTokenObject? tokenResponse;

        try
        {
            tokenResponse = await _sorfaceTokenHandler.RefreshTokenAsync(refreshToken, context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("token refresh error {}", ex.Message);
            return Result.RejectPrincipal;
        }

        switch (tokenResponse)
        {
            case null:
                _logger.Log(LogLevel.Error, "tokenResponse is null");

                return Result.RejectPrincipal;
            case { RefreshToken: null } or { AccessToken: null }:
                _logger.Log(LogLevel.Error, "tokenResponse is RefreshToken or AccessToken is null");

                return Result.RejectPrincipal;
        }

        var expirationValue = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn!.Value)
            .ToString("o", CultureInfo.InvariantCulture);

        var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var authenticateResultProperties = authenticateResult?.Properties;

        authenticateResultProperties?.StoreTokens(new[]
        {
            new AuthenticationToken { Name = RefreshTokenName, Value = tokenResponse.RefreshToken },
            new AuthenticationToken { Name = AccessTokenName, Value = tokenResponse.AccessToken },
            new AuthenticationToken { Name = ExpirationTokenName, Value = expirationValue },
        });

        if (authenticateResult is not null)
        {
            await context.SignInAsync(authenticateResult.Principal!, authenticateResult.Properties);
        }

        _logger.Log(LogLevel.Information, "Token refreshed");

        return Result.RefreshToken;
    }

    /// <summary>
    /// Enum for result operation by refresh token OAUTH2.
    /// </summary>
    public enum Result
    {
        /// <summary>
        /// Nothing. Refresh is not needed.
        /// </summary>
        None,

        /// <summary>
        /// Principal is rejected.
        /// </summary>
        RejectPrincipal,

        /// <summary>
        /// Token has been refreshed.
        /// </summary>
        RefreshToken,
    }
}
