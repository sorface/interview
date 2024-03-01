using Interview.Backend.Auth.Sorface;
using Interview.Backend.Responses;
using Interview.Backend.WebSocket.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace Interview.Backend.Auth;

public static class ServiceCollectionExt
{
    public static void AddAppAuth(this IServiceCollection self, AuthorizationService authorizationService, IConfiguration configuration)
    {
        var section = configuration
            .GetRequiredSection(nameof(SorfaceAuthenticationOptions))
            .Get<SorfaceAuthenticationFileOptions>() ?? throw new Exception("Unable parse SorfaceAuthenticationFileOptions");

        const string AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        self.AddAuthentication(AuthenticationScheme)
            .AddCookie(AuthenticationScheme, options =>
            {
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.WriteAsJsonAsync(new MessageResponse { Message = "Forbidden", });
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    context.Response.WriteAsJsonAsync(new MessageResponse { Message = "Unauthorized", });
                    return Task.CompletedTask;
                };
                options.Cookie.HttpOnly = false;
                options.Cookie.Name = WebSocketAuthorizationOptions.DefaultCookieName;
                options.ClaimsIssuer = authorizationService.ClaimsIssuer;
                options.ExpireTimeSpan = TimeSpan.FromDays(10);
            })
            .AddSorface(authorizationService.Id, options =>
            {
                options.ClaimsIssuer = section.Issuer;
                options.CallbackPath = section.CallbackPath;
                options.AuthorizationEndpoint = section.AuthorizationEndPoint;
                options.TokenEndpoint = section.TokenEndpoint;
                options.UserInformationEndpoint = section.UserInformationEndpoint;

                options.ClientId = authorizationService.ClientId;
                options.ClientSecret = authorizationService.ClientSecret;
                options.CallbackPath = authorizationService.CallbackPath;
                options.ClaimsIssuer = authorizationService.ClaimsIssuer;
                options.Events.OnTicketReceived += async context =>
                {
                    var user = context.Principal?.ToUser();
                    if (user == null)
                    {
                        return;
                    }

                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    var upsertUser = await userService.UpsertByTwitchIdentityAsync(user);
                    context.Principal!.EnrichRolesWithId(upsertUser);
                };
                options.Scope.Clear();
            });

        self.AddAuthorization(options =>
        {
            options.AddPolicy(SecurePolicy.Manager, policyBuilder =>
            {
                policyBuilder.RequireRole(RoleNameConstants.Admin);
            });
            options.AddPolicy(SecurePolicy.User, policyBuilder =>
            {
                policyBuilder.RequireAuthenticatedUser();
            });
        });
    }

    private sealed class SorfaceAuthenticationFileOptions
    {
        public string Issuer { get; set; } = string.Empty;

        public string CallbackPath { get; set; } = string.Empty;

        public string AuthorizationEndPoint { get; set; } = string.Empty;

        public string TokenEndpoint { get; set; } = string.Empty;

        public string UserInformationEndpoint { get; set; } = string.Empty;
    }
}
