using System.Text.Json;
using Interview.Infrastructure.WebSocket.PubSub.Events;
using StackExchange.Redis;

namespace Interview.Infrastructure.WebSocket.PubSub.Factory;

public class RedisPubSubFactoryConfiguration
{
    public required string ConnectionString { get; set; }
}

public sealed class RedisPubSubFactory : IPubSubFactory, IAsyncDisposable
{
    private readonly string _connectionString;
    private ConnectionMultiplexer? _connectionMultiplexer;

    public RedisPubSubFactory(RedisPubSubFactoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public async Task<IPubSub> CreateAsync(CancellationToken cancellationToken)
    {
        _connectionMultiplexer ??= await ConnectionMultiplexer.ConnectAsync(_connectionString);
        var subscriber = _connectionMultiplexer.GetSubscriber();
        return new PubSub(subscriber);
    }

    public ValueTask DisposeAsync()
    {
        return _connectionMultiplexer?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
    
    private sealed class PubSub : IPubSub
    {
        private readonly ISubscriber _subscriber;

        public PubSub(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public Task SubscribeAsync(IPubSubKey key, Action<RedisRoomEvent?> callback, CancellationToken cancellationToken)
        {
            return _subscriber.SubscribeAsync(CreateKey(key), (_, value) =>
            {
                if (!value.HasValue)
                {
                    callback(null);
                }
                else
                {
                    var content = (byte[])value.Box()!;
                    var ev = JsonSerializer.Deserialize<RedisRoomEvent>(content);
                    callback(ev);
                }
            });
        }

        public Task PublishAsync(IPubSubKey key, RedisRoomEvent roomEvent, CancellationToken cancellationToken)
        {
            var tKey = CreateKey(key);
            var serializeToUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(roomEvent);
            return _subscriber.PublishAsync(tKey, serializeToUtf8Bytes);
        }

        private static RedisChannel CreateKey(IPubSubKey key)
        {
            var buildStringKey = key.BuildStringKey();
            return new RedisChannel(buildStringKey, RedisChannel.PatternMode.Auto);
        }
    }
}
