using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Ardalis.SmartEnum.SystemTextJson;
using Interview.Backend.Auth;
using Interview.Backend.Swagger;
using Interview.Backend.WebSocket;
using Interview.Backend.WebSocket.Events;
using Interview.Backend.WebSocket.Events.ConnectionListener;
using Interview.Backend.WebSocket.Events.Handlers;
using Interview.DependencyInjection;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Microsoft.OpenApi.Models;

namespace Interview.Backend;

public class ServiceConfigurator
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ServiceConfigurator(IHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public static void AddWebSocketServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<RecyclableMemoryStreamManager>();
        serviceCollection.AddScoped<WebSocketReader>();
        serviceCollection.Scan(selector =>
        {
            selector.FromAssemblies(typeof(IWebSocketEventHandler).Assembly)
                .AddClasses(f => f.AssignableTo<IWebSocketEventHandler>())
                .As<IWebSocketEventHandler>()
                .WithScopedLifetime()
                .AddClasses(f => f.AssignableTo<IConnectionListener>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime();
        });
    }

    public void AddServices(IServiceCollection serviceCollection)
    {
        var corsOptions = _configuration.GetSection(nameof(CorsOptions)).Get<CorsOptions>();

        serviceCollection.AddCors(options =>
        {
            options.AddPolicy("All", policy =>
            {
                policy
                    .WithOrigins(corsOptions?.AllowedOrigins.ToArray() ?? Array.Empty<string>())
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

        serviceCollection.AddHostedService<EventSenderJob>();

        AddWebSocketServices(serviceCollection);

        serviceCollection.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        AddSwagger(serviceCollection);
        serviceCollection.AddHostedService<EventStorage2DatabaseBackgroundService>();
    }

    private void AddAppServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();

        var adminUsers = _configuration.GetSection(nameof(AdminUsers))
            .Get<AdminUsers>() ?? throw new ArgumentException($"Not found \"{nameof(AdminUsers)}\" section");

        var serviceOption = new DependencyInjectionAppServiceOption
        {
            DbConfigurator = optionsBuilder =>
            {
                var connectionString = _configuration.GetConnectionString("database");
                var database = _configuration.GetConnectionString("type")
                    ?.ToLower()
                    .Trim();
                var customDb = !string.IsNullOrWhiteSpace(database);

                if ((_environment.IsDevelopment() && !customDb) || "sqlite".Equals(database, StringComparison.InvariantCultureIgnoreCase))
                {
                    optionsBuilder.UseSqlite(
                        connectionString,
                        builder => builder.MigrationsAssembly(typeof(Migrations.Sqlite.AppDbContextFactory).Assembly
                            .FullName));
                }
                else if ((_environment.IsPreProduction() && !customDb) || "postgres".Equals(database, StringComparison.InvariantCultureIgnoreCase))
                {
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                    optionsBuilder.UseNpgsql(
                        connectionString,
                        builder => builder.MigrationsAssembly(typeof(Migrations.Postgres.AppDbContextFactory).Assembly
                            .FullName));
                }
                else
                {
                    throw new InvalidOperationException("Unknown environment");
                }
            },
            AdminUsers = adminUsers,
            EventStorageConfigurator = builder =>
            {
                RedisEnvironmentConfigure.Configure(_configuration,
                    (host, port, username, password) =>
                    {
                        builder.UseRedis($@"redis://{username}:{password}@{host}:{port}");
                    },
                    () =>
                    {
                        builder.UseEmpty();
                    });
            },
        };
        serviceCollection.AddAppServices(serviceOption);

        var openIdConnectOptions = _configuration.GetSection(nameof(OpenIdConnectOptions)).Get<OpenIdConnectOptions>()!;
        serviceCollection.AddAppAuth(openIdConnectOptions);
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

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            options.CustomSchemaIds(type => (type.FullName ?? type.Name).Replace("+", "_"));

            var swaggerOption = _configuration.GetSection(nameof(SwaggerOption)).Get<SwaggerOption>();
            if (!string.IsNullOrEmpty(swaggerOption?.RoutePrefix))
            {
                options.DocumentFilter<SwaggerDocumentFilter>(swaggerOption.RoutePrefix);
            }

            options.OperationFilter<DefaultResponseCodesFilter>();
        });
    }
}
