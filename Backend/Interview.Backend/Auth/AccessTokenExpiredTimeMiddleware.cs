using System.Globalization;
using Interview.Backend.Auth.Sorface;
using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth;

public class AccessTokenExpiredTimeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _cookieName;
    private readonly CookieOptions _cookieOptions;

    public AccessTokenExpiredTimeMiddleware(RequestDelegate next, string cookieName, CookieOptions cookieOptions)
    {
        _next = next;
        _cookieName = cookieName;
        _cookieOptions = cookieOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _next(context);
            return;
        }

        var expTimeString = await context.GetTokenAsync(SorfaceTokenDefaults.ExpirationTokenName);

        if (expTimeString is null)
        {
            await _next(context);
            return;
        }

        var expTime = ((DateTimeOffset)DateTime.Parse(expTimeString, CultureInfo.InvariantCulture).ToUniversalTime()).ToUnixTimeSeconds();

        context.Response.Cookies.Append(_cookieName, expTime.ToString(), _cookieOptions);

        await _next(context);
    }
}
