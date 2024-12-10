using Interview.Backend.Auth;
using Interview.Backend.Errors;
using Interview.Backend.WebSocket.Configuration;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

namespace Interview.Backend;

public class MiddlewareConfigurator(WebApplication app)
{
    public void AddMiddlewares()
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto, });
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseForwardedHeaders();
        if (app.Environment.IsPreProduction() || app.Environment.IsProduction())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax, HttpOnly = HttpOnlyPolicy.None, });

        app.UseWebSockets();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebSocketsAuthorization(new WebSocketAuthorizationOptions
            {
                CookieName = WebSocketAuthorizationOptions.DefaultCookieName,
                WebSocketQueryName = "Authorization",
            });
        }

        app.UseCors("All");

        app.UseRateLimiter();

        var logger = app.Services.GetRequiredService<ILogger<MiddlewareConfigurator>>();

        app.Use((context, func) =>
        {
            logger.LogInformation("Request {Path}", context.Request.Path);

            return func();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        var cookieExpTimeConfig = app.Configuration.GetSection("AccessTokenExpiredTime");
        app.UseAccessTokenExpiredTimeCookie("ate_t", new CookieOptions { Domain = cookieExpTimeConfig.GetValue<string>("Domain"), Secure = true, });

        app.Use((context, func) =>
        {
            var upsertUser = context.User.ToUser();
            logger.LogInformation(
                "Request {Path} authorized user [{nickname} {id}]",
                context.Request.Path,
                upsertUser?.Nickname,
                upsertUser?.Id);

            return func();
        });

        app.Use((context, func) =>
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

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();

        app.MapHealthChecks("/actuator/health/liveness", new HealthCheckOptions { Predicate = r => r.Name.Contains("liveness"), });
        app.MapHealthChecks("/actuator/health/readiness");
    }
}
