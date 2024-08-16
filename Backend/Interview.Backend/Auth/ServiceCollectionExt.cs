using Interview.Backend.Auth.Sorface;
using Interview.Backend.Responses;
using Interview.Domain.Users.Service;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Interview.Backend.Auth;

public static class ServiceCollectionExt
{
    public static void AddAppAuth(this IServiceCollection self, AuthorizationService authorizationService)
    {
        self.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = authorizationService.Id;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Events.OnValidatePrincipal = context =>
                {
                    var sorfacePrincipalValidator = context.HttpContext.RequestServices.GetRequiredService<SorfacePrincipalValidator>();
                    return sorfacePrincipalValidator.ValidateAsync(context);
                };
                options.Cookie.HttpOnly = false;
                options.Cookie.Name = "_auth";
                options.Cookie.Domain = authorizationService.Domain;

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
            })
            .AddSorface(authorizationService.Id, options =>
            {
                options.ClientId = authorizationService.ClientId;
                options.ClientSecret = authorizationService.ClientSecret;

                options.ClaimsIssuer = authorizationService.Issuer;

                options.CallbackPath = authorizationService.CallbackPath;

                options.AuthorizationEndpoint = authorizationService.AuthorizationEndPoint;
                options.TokenEndpoint = authorizationService.TokenEndpoint;
                options.UserInformationEndpoint = authorizationService.UserInformationEndpoint;

                options.SaveTokens = true;

                options.Scope.Add("scope.read");

                if (authorizationService.CorrelationCookie is not null)
                {
                    options.CorrelationCookie = new CookieBuilder
                    {
                        Name = authorizationService.CorrelationCookie.Name,
                        Domain = authorizationService.CorrelationCookie.Domain,
                    };
                }

                options.Events.OnTicketReceived += async context =>
                {
                    var user = context.Principal?.ToUser();
                    if (user is null)
                    {
                        return;
                    }

                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    var upsertUser = await userService.UpsertByExternalIdAsync(user);

                    context.Principal!.EnrichRolesWithId(upsertUser);
                };
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
}
