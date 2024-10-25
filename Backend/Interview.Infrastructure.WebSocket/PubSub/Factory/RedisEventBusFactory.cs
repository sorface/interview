using System.Text.Json;
using Interview.Infrastructure.WebSocket.PubSub.Events;
using StackExchange.Redis;

namespace Interview.Infrastructure.WebSocket.PubSub.Factory;

public class RedisPubSubFactoryConfiguration
{
    public required string ConnectionString { get; set; }
}

public sealed class RedisEventBusFactory : IEventBusPublisherFactory, IEventBusSubscriberFactory, IAsyncDisposable
{
    private readonly string _connectionString;
    private ConnectionMultiplexer? _connectionMultiplexer;

    public RedisEventBusFactory(RedisPubSubFactoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    async Task<IEventBusPublisher> IEventBusPublisherFactory.CreateAsync(CancellationToken cancellationToken)
    {
        _connectionMultiplexer ??= await ConnectionMultiplexer.ConnectAsync(_connectionString);
        var subscriber = _connectionMultiplexer.GetSubscriber();
        return new EventBus(subscriber);
    }

    async Task<IEventBusSubscriber> IEventBusSubscriberFactory.CreateAsync(CancellationToken cancellationToken)
    {
        _connectionMultiplexer ??= await ConnectionMultiplexer.ConnectAsync(_connectionString);
        var subscriber = _connectionMultiplexer.GetSubscriber();
        return new EventBus(subscriber);
    }
    
    public ValueTask DisposeAsync()
    {
        return _connectionMultiplexer?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
    
    private sealed class EventBus : IEventBus
    {
        private readonly ISubscriber _subscriber;

        public EventBus(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public async Task<IAsyncDisposable> SubscribeAsync<TKey>(TKey key, Action<TKey, EventBusEvent?> callback, CancellationToken cancellationToken)
            where TKey : IEventBusKey
        {
            var redisKey = CreateKey(key);
            await _subscriber.SubscribeAsync(redisKey, (_, value) =>
            {
                if (!value.HasValue)
                {
                    callback(key, null);
                }
                else
                {
                    var content = (byte[])value.Box()!;
                    var ev = JsonSerializer.Deserialize<EventBusEvent>(content);
                    callback(key, ev);
                }
            });
            
            return new Unsubscriber(_subscriber, redisKey);
        }

        public Task PublishAsync<TKey>(TKey key, EventBusEvent roomEventBusEvent, CancellationToken cancellationToken)
            where TKey : IEventBusKey
        {
            var tKey = CreateKey(key);
            var serializeToUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(roomEventBusEvent);
            return _subscriber.PublishAsync(tKey, serializeToUtf8Bytes);
        }

        private static RedisChannel CreateKey(IEventBusKey key)
        {
            var buildStringKey = key.BuildStringKey();
            return new RedisChannel(buildStringKey, RedisChannel.PatternMode.Auto);
        }
        
        private sealed class Unsubscriber : IAsyncDisposable
        {
            private readonly ISubscriber _subscriber;
            private readonly RedisChannel _key;

            public Unsubscriber(ISubscriber subscriber, RedisChannel key)
            {
                _subscriber = subscriber;
                _key = key;
            }

            public ValueTask DisposeAsync() => new(_subscriber.UnsubscribeAsync(_key));
        }
    }
}
