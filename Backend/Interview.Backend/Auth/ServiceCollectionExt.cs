using System.IdentityModel.Tokens.Jwt;
using Interview.Backend.Auth.Sorface;
using Interview.Backend.Responses;
using Interview.Backend.Users;
using Interview.Domain.Users.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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
        self.AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = "http://localhost:8080/.well-known/openid-configuration";
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.SaveToken = true;

                options.Events = new JwtBearerEvents
                {
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.WriteAsJsonAsync(new MessageResponse { Message = "Unauthorized", });
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 401;
                        context.Response.WriteAsJsonAsync(new MessageResponse { Message = "Unauthorized", });
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var user = context.Principal?.ToUser();

                        if (user is null)
                        {
                            return;
                        }

                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var upsertUser = await userService.UpsertByExternalIdAsync(user);

                        context.Principal!.EnrichRolesWithId(upsertUser);
                    },
                };

                options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false, };
            });

        self.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}
