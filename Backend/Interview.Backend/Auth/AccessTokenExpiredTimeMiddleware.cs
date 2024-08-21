using System.Globalization;
using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth
{
    public class AccessTokenExpiredTimeMiddleware
    {
        private const string ExpirationTokenName = "expires_at";
        private readonly RequestDelegate _next;
        private readonly string _cookieName;

        public AccessTokenExpiredTimeMiddleware(RequestDelegate next, string cookieName)
        {
            _next = next;
            _cookieName = cookieName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var expTimeString = await context.Response.HttpContext.GetTokenAsync(ExpirationTokenName);

            if (expTimeString is null)
            {
                await _next(context);
                return;
            }

            var expTime = ((DateTimeOffset)DateTime.Parse(expTimeString, CultureInfo.InvariantCulture).ToUniversalTime()).ToUnixTimeSeconds();

            context.Response.OnStarting(() => OnStartingCallBack(context, _cookieName, expTime));

            await _next(context);
        }

        private Task OnStartingCallBack(HttpContext context, string cookieName, long expTime)
        {
            var cookieOptions = new CookieOptions()
            {
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(1),
                IsEssential = true,
                HttpOnly = false,
                Secure = false,
            };
            context.Response.Cookies.Append(cookieName, expTime.ToString(), cookieOptions);
            return Task.FromResult(0);
        }
    }

    public static class AccessTokenExpiredTimeMiddlewareExtensions
    {
        public static IApplicationBuilder UseAccessTokenExpiredTimeCookie(this IApplicationBuilder builder, string cookieName)
        {
            return builder.UseMiddleware<AccessTokenExpiredTimeMiddleware>(cookieName);
        }
    }
}
