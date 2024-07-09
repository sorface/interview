using System.Globalization;
using Bogus.DataSets;
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

        try
        {
            var result = await HandleAsync(context);

            switch (result)
            {
                case Result.RefreshToken:
                    _logger.LogInformation("The token has been successfully updated");
                    break;
                case Result.RejectPrincipal:
                    return;
                case Result.None:
                    _logger.LogInformation("The token does not need to be updated");
                    break;
                default:
                    _logger.Log(LogLevel.Error, "tokenResponse is RefreshToken or AccessToken is null");
                    throw new ArgumentNullException(nameof(result));
            }

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
                _logger.LogInformation(@"Token refresh operation has been with result {result}", result);
            }
        }
        catch (Exception e)
        {
            context.RejectPrincipal();
            _logger.LogError(e, "during token validation");
        }
    }

    private async Task<Result> HandleAsync(CookieValidatePrincipalContext context)
    {
        if (context.Principal?.Identity is { IsAuthenticated: false })
        {
            _logger.Log(LogLevel.Information, "Principal is not authenticated");
            return Result.None;
        }

        var exp = context.Properties.GetTokenValue(ExpirationTokenName);

        if (exp is null)
        {
            _logger.Log(LogLevel.Information, "Exp is null. not refreshed");
            return Result.None;
        }

        _logger.LogInformation(@"get exp {exp}", exp);

        var expires = DateTime.Parse(exp, CultureInfo.InvariantCulture).ToUniversalTime();

        _logger.LogInformation(@"utc exp {expires}", expires);
        if (expires > DateTime.UtcNow)
        {
            _logger.Log(LogLevel.Information, "expires >= DateTime.UtcNow");
            return Result.None;
        }

        var refreshToken = context.Properties.GetTokenValue(RefreshTokenName);

        if (refreshToken is null)
        {
            _logger.Log(LogLevel.Warning, "refreshToken is null");
            context.RejectPrincipal();
            return Result.RejectPrincipal;
        }

        var tokenResponse = await _sorfaceTokenHandler.RefreshTokenAsync(refreshToken, context.HttpContext.RequestAborted);

        switch (tokenResponse)
        {
            case null:
                _logger.Log(LogLevel.Error, "tokenResponse is null");

                context.RejectPrincipal();
                return Result.RejectPrincipal;
            case { RefreshToken: null } or { AccessToken: null }:
                _logger.Log(LogLevel.Error, "tokenResponse is RefreshToken or AccessToken is null");

                context.RejectPrincipal();
                return Result.RejectPrincipal;
        }

        var expirationValue = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn!.Value)
            .ToString("o", CultureInfo.InvariantCulture);

        context.Properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = RefreshTokenName, Value = tokenResponse.RefreshToken },
            new AuthenticationToken { Name = AccessTokenName, Value = tokenResponse.AccessToken },
            new AuthenticationToken { Name = ExpirationTokenName, Value = expirationValue },
        });

        _logger.Log(LogLevel.Information, "Token refreshed");

        return Result.RefreshToken;
    }

    /// <summary>
    /// Enum for result operation by refresh token OAUTH2.
    /// </summary>
    private enum Result
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
