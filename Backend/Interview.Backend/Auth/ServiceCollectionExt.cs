using Interview.Backend.Responses;
using Interview.Domain.Users.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Interview.Backend.Auth;

public static class ServiceCollectionExt
{
    public static void AddAppAuth(this IServiceCollection self, OpenIdConnectOptions openIdConnectOptions)
    {
        self.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = $@"{openIdConnectOptions.Issuer}{openIdConnectOptions.MetadataPath}";
                options.RequireHttpsMetadata = openIdConnectOptions.RequireHttpsMetadata;
                options.MapInboundClaims = false;
                options.SaveToken = false;

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
