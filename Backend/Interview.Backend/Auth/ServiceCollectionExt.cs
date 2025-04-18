using Interview.Backend.Auth.Dev;
using Interview.Domain;
using Interview.Domain.Users.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Interview.Backend.Auth;

public static class ServiceCollectionExt
{
    public static void AddAppAuth(this IServiceCollection self, IHostEnvironment environment, OpenIdConnectOptions openIdConnectOptions)
    {
        if (environment.IsDevelopment())
        {
            AddDev(self);
        }
        else
        {
            AddProd(self, openIdConnectOptions);
        }
    }

    private static void AddDev(IServiceCollection self)
    {
        var defaultScheme = "DevBearer";
        self
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = defaultScheme;
                options.DefaultChallengeScheme = defaultScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Audience = "Audience";
                options.Authority = "Authority";
                options.RequireHttpsMetadata = false;
                options.Events = new JwtBearerEvents
                {
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
            });

        self.AddAuthentication(defaultScheme)
            .AddScheme<DevelopmentAuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(defaultScheme, null);

        // This is custom and you might need change it to your needs.
        self.AddAuthorization(option =>
        {
            option.AddPolicy("DevBearer", builder =>
            {
                builder.RequireAuthenticatedUser();
            });
        });
    }

    private static void AddProd(this IServiceCollection self, OpenIdConnectOptions openIdConnectOptions)
    {
        self.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = $@"{openIdConnectOptions.Issuer}{openIdConnectOptions.MetadataPath}";
                options.RequireHttpsMetadata = openIdConnectOptions.RequireHttpsMetadata;
                options.MapInboundClaims = false;
                options.SaveToken = true;

                options.Events = new JwtBearerEvents
                {
                    OnForbidden = _ => throw new AccessDeniedException("Forbidden"),
                    OnAuthenticationFailed = _ => throw new UnauthorizedAccessException(),
                    OnTokenValidated = async context =>
                    {
                        var user = context.Principal?.ToUser();

                        if (user is null)
                        {
                            return;
                        }

                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();

                        var upsertUser = await userService.UpsertByExternalIdAsync(user)!;

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
