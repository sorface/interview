using System.Globalization;
using Interview.Backend.Auth.Sorface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Interview.Backend.Auth
{
    public static class HttpContextOAuth2Extensions
    {
        public static async Task<Result> RefreshTokenAsync(this HttpContext context, CancellationToken cancellationToken)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<HttpContext>>();

            var exp = await context.GetTokenAsync(SorfaceTokenDefaults.ExpirationTokenName);

            if (exp is null)
            {
                logger.Log(LogLevel.Information, "Exp is null. not refreshed");
                return Result.None;
            }

            logger.LogInformation(@"get exp {exp}", exp);

            var refreshToken = await context.GetTokenAsync(SorfaceTokenDefaults.RefreshTokenName);

            if (refreshToken is null)
            {
                logger.Log(LogLevel.Warning, "refreshToken is null");
                return Result.RejectPrincipal;
            }

            RefreshTokenObject? tokenResponse;

            try
            {
                var sorfaceAuthenticationHandler = context.RequestServices.GetRequiredService<SorfaceTokenService>();

                tokenResponse = await sorfaceAuthenticationHandler.RefreshTokenAsync(refreshToken, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogDebug("token refresh error {}", ex.Message);
                return Result.RejectPrincipal;
            }

            switch (tokenResponse)
            {
                case null:
                    logger.Log(LogLevel.Error, "tokenResponse is null");

                    return Result.RejectPrincipal;
                case { RefreshToken: null } or { AccessToken: null }:
                    logger.Log(LogLevel.Error, "tokenResponse is RefreshToken or AccessToken is null");

                    return Result.RejectPrincipal;
            }

            var expirationValue = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn!.Value)
                .ToString("o", CultureInfo.InvariantCulture);

            var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var authenticateResultProperties = authenticateResult?.Properties;

            authenticateResultProperties?.StoreTokens(new[]
            {
                new AuthenticationToken { Name = SorfaceTokenDefaults.RefreshTokenName, Value = tokenResponse.RefreshToken },
                new AuthenticationToken { Name = SorfaceTokenDefaults.AccessTokenName, Value = tokenResponse.AccessToken },
                new AuthenticationToken { Name = SorfaceTokenDefaults.ExpirationTokenName, Value = expirationValue },
            });

            if (authenticateResultProperties is not null)
            {
                authenticateResultProperties.AllowRefresh = true;
                var dateTimeOffset = authenticateResultProperties.ExpiresUtc;

                logger.LogInformation("authenticateResultProperties dateTimeOffset: {}", dateTimeOffset);
            }

            if (authenticateResult is not null)
            {
                await context.SignInAsync(authenticateResult.Principal!, authenticateResult.Properties);
            }

            logger.Log(LogLevel.Information, "Token refreshed");

            return Result.RefreshToken;
        }
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
