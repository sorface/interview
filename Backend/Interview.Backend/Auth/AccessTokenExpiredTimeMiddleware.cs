using System.Globalization;
using Interview.Backend.Auth.Sorface;
using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth
{
    public static class AccessTokenExpiredTimeMiddlewareExtensions
    {
        public static IApplicationBuilder UseAccessTokenExpiredTimeCookie(this IApplicationBuilder builder, string cookieName)
        {
            return builder.UseMiddleware<AccessTokenExpiredTimeMiddleware>(cookieName);
        }
    }

    public class AccessTokenExpiredTimeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _cookieName;

        public AccessTokenExpiredTimeMiddleware(RequestDelegate next, string cookieName)
        {
            _next = next;
            _cookieName = cookieName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var expTimeString = await context.GetTokenAsync(SorfaceTokenDefaults.ExpirationTokenName);

            if (expTimeString is null)
            {
                await _next(context);
                return;
            }

            var expTime = ((DateTimeOffset)DateTime.Parse(expTimeString, CultureInfo.InvariantCulture).ToUniversalTime()).ToUnixTimeSeconds();

            context.Response.Cookies.Append(_cookieName, expTime.ToString());

            await _next(context);
        }
    }
}
