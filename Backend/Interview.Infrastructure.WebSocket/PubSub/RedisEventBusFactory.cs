using System.Text.Json;
using Interview.Domain.PubSub;
using Interview.Domain.PubSub.Events;
using Interview.Domain.PubSub.Factory;
using StackExchange.Redis;

namespace Interview.Infrastructure.WebSocket.PubSub;

public sealed class RedisEventBusFactory(RedisPubSubFactoryConfiguration configuration) : IEventBusPublisherFactory, IEventBusSubscriberFactory, IAsyncDisposable
{
    private readonly ConfigurationOptions _configuration = configuration.Configuration;
    private ConnectionMultiplexer? _connectionMultiplexer;

    public ValueTask DisposeAsync()
    {
        return _connectionMultiplexer?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    async Task<IEventBusPublisher> IEventBusPublisherFactory.CreateAsync(CancellationToken cancellationToken)
    {
        _connectionMultiplexer ??= await CreateConnectionMultiplexerAsync();
        var subscriber = _connectionMultiplexer.GetSubscriber();
        return new EventBus(subscriber);
    }

    async Task<IEventBusSubscriber> IEventBusSubscriberFactory.CreateAsync(CancellationToken cancellationToken)
    {
        _connectionMultiplexer ??= await CreateConnectionMultiplexerAsync();
        var subscriber = _connectionMultiplexer.GetSubscriber();
        return new EventBus(subscriber);
    }

    private Task<ConnectionMultiplexer> CreateConnectionMultiplexerAsync()
    {
        return ConnectionMultiplexer.ConnectAsync(_configuration);
    }

    private sealed class EventBus(ISubscriber subscriber) : IEventBus
    {
        public Task<IAsyncDisposable> SubscribeAsync<TKey>(TKey key, Action<TKey, EventBusEvent?> callback, CancellationToken cancellationToken)
            where TKey : IEventBusKey
        {
            var redisKey = CreateKey(key);
            Action<RedisChannel, RedisValue> handler = (_, value) =>
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
            };
            subscriber.Subscribe(redisKey, handler);

            return Task.FromResult<IAsyncDisposable>(new Unsubscriber(subscriber, redisKey, handler));
        }

        public Task PublishAsync<TKey>(TKey key, EventBusEvent roomEventBusEvent, CancellationToken cancellationToken)
            where TKey : IEventBusKey
        {
            var tKey = CreateKey(key);
            var serializeToUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(roomEventBusEvent);
            return subscriber.PublishAsync(tKey, serializeToUtf8Bytes);
        }

        private static RedisChannel CreateKey(IEventBusKey key)
        {
            var buildStringKey = key.BuildStringKey();
            return new RedisChannel(buildStringKey, RedisChannel.PatternMode.Auto);
        }

        private sealed class Unsubscriber(ISubscriber subscriber, RedisChannel key, Action<RedisChannel, RedisValue> handler) : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => new(subscriber.UnsubscribeAsync(key, handler));
        }
    }
}

public class RedisPubSubFactoryConfiguration
{
    public required ConfigurationOptions Configuration { get; set; }
}
