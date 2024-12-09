using System.Globalization;
using Interview.Backend.Auth.Sorface;
using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth;

public class AccessTokenExpiredTimeMiddleware(RequestDelegate next, string cookieName, CookieOptions cookieOptions)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await next(context);
            return;
        }

        var expTimeString = await context.GetTokenAsync(SorfaceTokenDefaults.ExpirationTokenName);

        if (expTimeString is null)
        {
            await next(context);
            return;
        }

        var expTime = ((DateTimeOffset)DateTime.Parse(expTimeString, CultureInfo.InvariantCulture).ToUniversalTime()).ToUnixTimeSeconds();

        context.Response.Cookies.Append(cookieName, expTime.ToString(), cookieOptions);

        await next(context);
    }
}
