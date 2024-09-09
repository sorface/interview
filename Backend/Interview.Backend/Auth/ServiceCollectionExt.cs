using Interview.Backend.Auth.Sorface;
using Interview.Backend.Responses;
using Interview.Backend.Users;
using Interview.Domain.Users.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Interview.Backend.Auth;

public static class ServiceCollectionExt
{
    private static readonly Dictionary<Type, string[]> DISABLEDCONTROLLER = new()
    {
        [typeof(AuthController)] = new[] { nameof(AuthController.SignIn), nameof(AuthController.SignOutImpl) },
        [typeof(UserController)] = new[] { nameof(UserController.GetMyself) },
    };

    public static void AddAppAuth(this IServiceCollection self, AuthorizationService authorizationService)
    {
        self.AddSingleton<SemaphoreLockProvider<string>>();
        self.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Events.OnValidatePrincipal = context =>
                {
                    var descriptor = context.HttpContext.GetEndpoint()?.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();

                    if (descriptor != null && DISABLEDCONTROLLER.TryGetValue(descriptor.ControllerTypeInfo, out var exception) &&
                        exception.Contains(descriptor.MethodInfo.Name))
                    {
                        return Task.CompletedTask;
                    }

                    var sorfacePrincipalValidator = context.HttpContext.RequestServices.GetRequiredService<SorfacePrincipalValidator>();
                    return sorfacePrincipalValidator.ValidateAsync(context);
                };

                options.SessionStore = self.BuildServiceProvider().GetRequiredService<ITicketStore>();
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = authorizationService.CookieName;
                options.Cookie.Domain = authorizationService.CookieDomain;

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
                    var cookieBuilder = new CookieBuilder();

                    if (authorizationService.CorrelationCookie?.Name is not null && authorizationService.CorrelationCookie?.Name.Length > 0)
                    {
                        cookieBuilder.Name = authorizationService.CorrelationCookie.Name;
                    }

                    if (authorizationService.CorrelationCookie?.Domain is not null && authorizationService.CorrelationCookie?.Domain.Length > 0)
                    {
                        cookieBuilder.Domain = authorizationService.CorrelationCookie.Domain;
                    }

                    options.CorrelationCookie = cookieBuilder;
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
