using Interview.Domain.PubSub.Factory;
using Interview.Infrastructure.WebSocket;
using Interview.Infrastructure.WebSocket.Events;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Interview.Infrastructure.WebSocket.Events.Handlers;
using Interview.Infrastructure.WebSocket.PubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IO;

namespace Interview.Infrastructure.WebSockets;

/// <summary>
/// ServiceCollectionExt.
/// </summary>
public static class ServiceCollectionExt
{
    public static IServiceCollection AddWebSocketServices(
        this IServiceCollection self,
        Action<PubSubFactoryConfiguration> pubSubFactoryConfiguration)
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

        self.AddHostedService<EventStorage2DatabaseBackgroundService>();

        var subFactoryConfiguration = new PubSubFactoryConfiguration();
        pubSubFactoryConfiguration.Invoke(subFactoryConfiguration);
        subFactoryConfiguration.AddServices(self);
        return self;
    }
}

public sealed class PubSubFactoryConfiguration
{
    private Type? _implementation;
    private RedisPubSubFactoryConfiguration? _configuration;

    public void UseInMemory()
    {
        _configuration = null;
        _implementation = typeof(MemoryEventBusFactory);
    }

    public void UseRedis(RedisPubSubFactoryConfiguration configuration)
    {
        _configuration = configuration;
        _implementation = typeof(RedisEventBusFactory);
    }

    internal void AddServices(IServiceCollection serviceCollection)
    {
        if (_implementation is null)
        {
            throw new Exception("You should specify implementation of PubSubFactory");
        }

        serviceCollection.AddSingleton(_implementation);
        serviceCollection.AddSingleton<IEventBusPublisherFactory>(provider => (IEventBusPublisherFactory)provider.GetRequiredService(_implementation));
        serviceCollection.AddSingleton<IEventBusSubscriberFactory>(provider => (IEventBusSubscriberFactory)provider.GetRequiredService(_implementation));
        serviceCollection.AddSingleton<HandleStatefulEventHandler>();
        if (_configuration is not null)
        {
            serviceCollection.AddSingleton(_configuration);
        }
    }
}
