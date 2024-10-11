using Interview.Backend.Auth;
using Interview.Backend.Errors;
using Interview.Backend.WebSocket.Configuration;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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

        _app.UseCors("All");

        _app.UseRateLimiter();

        var logger = _app.Services.GetRequiredService<ILogger<MiddlewareConfigurator>>();

        _app.Use((context, func) =>
        {
            logger.LogInformation("Request {Path}", context.Request.Path);

            return func();
        });

        _app.UseSwagger();
        _app.UseSwaggerUI();

        _app.UseAuthentication();
        _app.UseAuthorization();

        var cookieExpTimeConfig = _app.Configuration.GetSection("AccessTokenExpiredTime");
        _app.UseAccessTokenExpiredTimeCookie("ate_t", new CookieOptions { Domain = cookieExpTimeConfig.GetValue<string>("Domain"), Secure = true, });

        _app.Use((context, func) =>
        {
            var upsertUser = context.User.ToUser();
            logger.LogInformation(
                "Request {Path} authorized user [{nickname} {id}] TIME: {time}",
                context.Request.Path,
                upsertUser?.Nickname,
                upsertUser?.Id,
                UnixTime());

            static long UnixTime()
            {
                var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return (DateTime.UtcNow - epochStart).Ticks * 100;
            }

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
        
        _app.MapControllers();

        _app.MapHealthChecks("/actuator/health/liveness", new HealthCheckOptions { Predicate = r => r.Name.Contains("liveness"), });
        _app.MapHealthChecks("/actuator/health/readiness");
    }
}
