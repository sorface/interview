using System.Globalization;
using Interview.Backend.Auth;
using Interview.Backend.Errors;
using Interview.Backend.WebSocket.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.CookiePolicy;

namespace Interview.Backend;

public class MiddlewareConfigurator
{
    private readonly WebApplication _app;

    public MiddlewareConfigurator(WebApplication app)
    {
        _app = app;
    }

    public void AddMiddlewares()
    {
        _app.UseMiddleware<ExceptionMiddleware>();
        _app.UseForwardedHeaders();
        if (_app.Environment.IsPreProduction() || _app.Environment.IsProduction())
        {
            _app.UseHsts();
        }

        _app.UseHttpsRedirection();

        _app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax, HttpOnly = HttpOnlyPolicy.None, });

        _app.UseWebSockets();

        if (_app.Environment.IsDevelopment())
        {
            _app.UseWebSocketsAuthorization(new WebSocketAuthorizationOptions
            {
                CookieName = WebSocketAuthorizationOptions.DefaultCookieName,
                WebSocketQueryName = "Authorization",
            });
        }

        _app.UseCors("All");

        _app.UseRateLimiter();

        var logger = _app.Services.GetRequiredService<ILogger<MiddlewareConfigurator>>();

        _app.Use((context, func) =>
        {
            logger.LogInformation("Request {Path}", context.Request.Path);

            return func();
        });

        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.UseSession();

        var cookieExpTimeConfig = _app.Configuration.GetSection("AccessTokenExpiredTime");
        _app.UseAccessTokenExpiredTimeCookie("ate_t", new CookieOptions { Domain = cookieExpTimeConfig.GetValue<string>("Domain"), Secure = true, });

        _app.Use((context, func) =>
        {
            var upsertUser = context.User.ToUser();
            logger.LogInformation(
                "Request {Path} authorized user [{nickname} {id}]. session id: {sessionId}",
                context.Request.Path,
                upsertUser?.Nickname,
                upsertUser?.Id,
                context.Session.Id);

            return func();
        });

        _app.Use((context, func) =>
        {
            var upsertUser = context.User.ToUser();

            if (upsertUser is null)
            {
                return func();
            }

            var userAccessor = context.RequestServices.GetRequiredService<IEditableCurrentUserAccessor>();
            userAccessor.SetUser(upsertUser);

            return func();
        });

        _app.UseSwagger();
        _app.UseSwaggerUI();

        _app.MapControllers();
    }
}
