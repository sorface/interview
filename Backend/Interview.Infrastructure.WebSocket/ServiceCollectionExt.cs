using Interview.Infrastructure.WebSocket.Events;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Interview.Infrastructure.WebSocket.Events.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IO;

namespace Interview.Infrastructure.WebSocket;

/// <summary>
/// ServiceCollectionExt.
/// </summary>
public static class ServiceCollectionExt
{
    public static IServiceCollection AddWebSocketServices(this IServiceCollection self)
    {
        self.AddScoped<WebSocketConnectionHandler>();
        self.TryAddSingleton(typeof(RecyclableMemoryStreamManager));
        self.AddScoped<WebSocketReader>();
        self.Scan(selector =>
        {
            selector.FromAssemblies(typeof(IWebSocketEventHandler).Assembly)
                .AddClasses(f => f.AssignableTo<IWebSocketEventHandler>())
                .As<IWebSocketEventHandler>()
                .WithScopedLifetime()
                .AddClasses(f => f.AssignableTo<IConnectionListener>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime();
        });

        self.AddHostedService<EventSenderJob>();
        self.AddHostedService<EventStorage2DatabaseBackgroundService>();
        return self;
    }
}
