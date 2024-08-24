namespace Interview.Backend.Auth;

public static class AccessTokenExpiredTimeMiddlewareExtensions
{
    public static IApplicationBuilder UseAccessTokenExpiredTimeCookie(this IApplicationBuilder builder, string cookieName)
    {
        return builder.UseMiddleware<AccessTokenExpiredTimeMiddleware>(cookieName);
    }
}
