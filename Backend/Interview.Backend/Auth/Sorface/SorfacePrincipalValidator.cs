using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Interview.Backend.Auth.Sorface;

public class SorfacePrincipalValidator
{
    private readonly ILogger<SorfacePrincipalValidator> _logger;
    private readonly SorfaceTokenService _sorfaceTokenService;
    private readonly SemaphoreLockProvider<string> _semaphoreLockProvider;

    public SorfacePrincipalValidator(ILogger<SorfacePrincipalValidator> logger,
                                     SorfaceTokenService sorfaceTokenService,
                                     SemaphoreLockProvider<string> semaphoreLockProvider)
    {
        _logger = logger;
        _sorfaceTokenService = sorfaceTokenService;
        _semaphoreLockProvider = semaphoreLockProvider;
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
            if (context.Principal?.Identity is { IsAuthenticated: false })
            {
                _logger.Log(LogLevel.Information, "Principal is not authenticated");
                return;
            }

            var exp = context.Properties.GetTokenValue(SorfaceTokenDefaults.ExpirationTokenName);

            if (exp is null)
            {
                _logger.Log(LogLevel.Information, "Exp is null. not refreshed");
                return;
            }

            var expTime = DateTime.Parse(exp, CultureInfo.InvariantCulture).ToUniversalTime();

            if (expTime > DateTime.UtcNow)
            {
                _logger.Log(LogLevel.Information, "Introspect access token");

                await IntrospectToken(context);
            }
            else
            {
                var refreshToken = context.Properties.GetTokenValue(SorfaceTokenDefaults.RefreshTokenName);
                if (refreshToken is null)
                {
                    _logger.Log(LogLevel.Warning, "refreshToken is null");
                    context.RejectPrincipal();
                    return;
                }

                await _semaphoreLockProvider.WaitAsync(refreshToken, context.HttpContext.RequestAborted);

                try
                {
                    _logger.LogDebug($@"Start refresh ACCESS_TOKEN with lock");

                    await RefreshAccessTokenAsync(context, refreshToken, context.HttpContext.RequestAborted);

                    _logger.LogDebug($@"End refresh ACCESS_TOKEN unlock");
                }
                finally
                {
                    _semaphoreLockProvider.Release(refreshToken);
                }
            }
        }
        catch (Exception e)
        {
            context.RejectPrincipal();
            _logger.LogError(e, "during token validation");
        }
    }

    private async Task IntrospectToken(CookieValidatePrincipalContext context)
    {
        var accessToken = context.Properties.GetTokenValue(SorfaceTokenDefaults.AccessTokenName);

        if (accessToken is null)
        {
            context.RejectPrincipal();
            return;
        }

        using var tokenPrincipal = await _sorfaceTokenService.GetTokenPrincipalAsync(accessToken, context.HttpContext.RequestAborted);

        if (!tokenPrincipal.RootElement.TryGetProperty("active", out var active))
        {
            context.RejectPrincipal();
            return;
        }

        if (!active.GetBoolean())
        {
            _logger.LogInformation(@"Token refresh operation has been with result {result}", tokenPrincipal);
            context.RejectPrincipal();
            return;
        }

        _logger.LogDebug($@"Access token success introspected");
    }

    private async Task RefreshAccessTokenAsync(CookieValidatePrincipalContext context, string refreshToken, CancellationToken cancellationToken)
    {
        RefreshTokenObject? tokenResponse;

        try
        {
            tokenResponse = await _sorfaceTokenService.RefreshTokenAsync(refreshToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("token refresh error {}", ex.Message);
            return;
        }

        switch (tokenResponse)
        {
            case null:
                return;
            case { RefreshToken: null } or { AccessToken: null }:
                return;
        }

        var expirationValue = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn!.Value)
            .ToString("o", CultureInfo.InvariantCulture);

        context.Properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = SorfaceTokenDefaults.RefreshTokenName, Value = tokenResponse.RefreshToken },
            new AuthenticationToken { Name = SorfaceTokenDefaults.AccessTokenName, Value = tokenResponse.AccessToken },
            new AuthenticationToken { Name = SorfaceTokenDefaults.ExpirationTokenName, Value = expirationValue },
        });

        context.ShouldRenew = true;
    }
}
