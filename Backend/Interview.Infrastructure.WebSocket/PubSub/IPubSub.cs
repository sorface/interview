using System.Text.Json;
using Interview.Infrastructure.WebSocket.PubSub.Events;
using StackExchange.Redis;

namespace Interview.Infrastructure.WebSocket.PubSub;

public interface IPubSub
{
    Task SubscribeAsync(IPubSubKey key, Action<RedisRoomEvent?> callback, CancellationToken cancellationToken);
    Task PublishAsync(IPubSubKey key, RedisRoomEvent roomEvent, CancellationToken cancellationToken);
}

public sealed class PubSub : IPubSub
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
