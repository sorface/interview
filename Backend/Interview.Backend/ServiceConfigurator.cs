using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Ardalis.SmartEnum.SystemTextJson;
using Interview.Backend.Auth;
using Interview.Backend.Auth.Sorface;
using Interview.Backend.Swagger;
using Interview.DependencyInjection;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Infrastructure.WebSocket;
using Interview.Infrastructure.WebSocket.PubSub;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;

namespace Interview.Backend;

public class ServiceConfigurator(IHostEnvironment environment, IConfiguration configuration, ILogger logger)
{
    public void AddServices(IServiceCollection serviceCollection)
    {
        var corsOptions = configuration.GetSection(nameof(CorsOptions)).Get<CorsOptions>();

        logger.Information("CORS: {AllowedOrigins}", corsOptions?.AllowedOrigins);
        serviceCollection.AddCors(options =>
        {
            options.AddPolicy("All", policy =>
            {
                policy
                    .WithOrigins(corsOptions?.AllowedOrigins.ToArray() ?? [])
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        serviceCollection
            .AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new SmartEnumNameConverter<RoleName, int>());
                options.JsonSerializerOptions.Converters.Add(new SmartEnumNameConverter<RoomQuestionState, int>());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();

        AddAppServices(serviceCollection);

        serviceCollection.AddSingleton(new OAuthServiceDispatcher(configuration));

        serviceCollection.AddWebSocketServices(configuration1 =>
        {
            RedisEnvironmentConfigure.Configure(configuration,
                (host, port, username, password) =>
                {
                    var configurationOptions = ConfigurationOptions.Parse($@"{host}:{port},password={password}");
                    configuration1.UseRedis(new RedisPubSubFactoryConfiguration
                    {
                        Configuration = configurationOptions,
                    });
                },
                configuration1.UseInMemory);
        });

        serviceCollection.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
        AddRateLimiter(serviceCollection);

        AddSwagger(serviceCollection);
    }

    private void AddAppServices(IServiceCollection serviceCollection)
    {
        var sorfaceAuth = new OAuthServiceDispatcher(configuration).GetAuthService("sorface") ?? throw new Exception("Not found \"sorface\" section");

        serviceCollection.AddSingleton(sorfaceAuth);
        serviceCollection.AddHttpClient();
        serviceCollection.AddSingleton<SorfacePrincipalValidator>();
        serviceCollection.AddSingleton<SorfaceTokenService>();

        var adminUsers = configuration.GetSection(nameof(AdminUsers))
            .Get<AdminUsers>() ?? throw new ArgumentException($"Not found \"{nameof(AdminUsers)}\" section");

        var dbProvider = GetDbProvider();
        var serviceOption = new DependencyInjectionAppServiceOption
        {
            DbConfigurator = optionsBuilder =>
            {
                var connectionString = configuration.GetConnectionString("database");

                switch (dbProvider)
                {
                    case DbProvider.Sqlite:
                        optionsBuilder.UseSqlite(
                            connectionString,
                            builder => builder.MigrationsAssembly(typeof(Migrations.Sqlite.AppDbContextFactory).Assembly.FullName));
                        break;
                    case DbProvider.Postgres:
                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                        optionsBuilder.UseNpgsql(
                            connectionString,
                            builder => builder.MigrationsAssembly(typeof(Migrations.Postgres.AppDbContextFactory).Assembly.FullName));
                        break;
                    default:
                        throw new InvalidOperationException("Unknown environment");
                }
            },
            AdminUsers = adminUsers,
            EventStorageConfigurator = builder =>
            {
                RedisEnvironmentConfigure.Configure(configuration,
                    (host, port, username, password) =>
                    {
                        logger.Information("Use redis event storage");
                        builder.UseRedis($@"redis://{username}:{password}@{host}:{port}");

                        serviceCollection.AddStackExchangeRedisCache(options =>
                        {
                            options.Configuration = $@"{host}:{port},password={password}";
                            options.InstanceName = "sorface.interview.session.";
                        });
                    },
                    () =>
                    {
                        logger.Information("Use memory event storage");
                        serviceCollection.AddDistributedMemoryCache();
                        builder.UseEmpty();
                    });
            },
        };

        serviceCollection.AddAppServices(serviceOption);

        serviceCollection.AddSingleton<IDistributedLockStorage, DistributedLockStorage>();

        serviceCollection
            .AddSingleton<ITicketStore, DistributedCacheTicketStore>()
            .AddSession()
            .AddAppAuth(environment, sorfaceAuth);
    }

    private void AddRateLimiter(IServiceCollection serviceCollection)
    {
        serviceCollection.AddRateLimiter(_ =>
        {
            _.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
            {
                /*
                var address = context.Connection.RemoteIpAddress;
                if (address is not null && !IPAddress.IsLoopback(address))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(address, key => new()
                    {
                        PermitLimit = 36,
                        Window = TimeSpan.FromSeconds(30),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true,
                    });
                }
                */
                return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
            });
            _.OnRejected = (OnRejectedContext context, CancellationToken token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.",
                    cancellationToken: token);

                return ValueTask.CompletedTask;
            };
        });
    }

    private void AddSwagger(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Open API",
                Version = "v1",
                Description = "Service Interface",
                Contact = new OpenApiContact { Name = "Vladislav Petyukevich", Url = new Uri("https://github.com/VladislavPetyukevich"), Email = "test@yandex.ru", },
                License = new OpenApiLicense { Name = "Example License", Url = new Uri("https://example.com/license"), },
            });

            if (environment.IsDevelopment())
            {
                options.AddSecurityDefinition("DevBearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. Example: 'DevBearer TEST_BACKEND_DEV_USER'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "DevBearer",
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "DevBearer",
                            },
                            Scheme = "oauth2",
                            Name = "DevBearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    },
                });
            }

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            options.CustomSchemaIds(type => (type.FullName ?? type.Name).Replace("+", "_"));

            var swaggerOption = configuration.GetSection(nameof(SwaggerOption)).Get<SwaggerOption>();
            if (!string.IsNullOrEmpty(swaggerOption?.RoutePrefix))
            {
                options.DocumentFilter<SwaggerDocumentFilter>(swaggerOption.RoutePrefix);
            }

            options.OperationFilter<DefaultResponseCodesFilter>();
        });
    }

    private DbProvider GetDbProvider()
    {
        var database = configuration.GetConnectionString("type")
            ?.ToLower()
            .Trim();
        var customDb = !string.IsNullOrWhiteSpace(database);

        if ((environment.IsDevelopment() && !customDb) || "sqlite".Equals(database, StringComparison.InvariantCultureIgnoreCase))
        {
            logger.Information("Use sqlite");
            return DbProvider.Sqlite;
        }

        if ((environment.IsPreProduction() && !customDb) || "postgres".Equals(database, StringComparison.InvariantCultureIgnoreCase))
        {
            logger.Information("Use postgres");
            return DbProvider.Postgres;
        }

        return DbProvider.Unknown;
    }

    private enum DbProvider
    {
        Unknown,
        Postgres,
        Sqlite,
    }
}
