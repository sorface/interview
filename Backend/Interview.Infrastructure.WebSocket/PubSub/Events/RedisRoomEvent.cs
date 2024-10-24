using System.Text.Json;

namespace Interview.Infrastructure.WebSocket.PubSub.Events;

[JsonDerivedTypeByName<ReceivedRedisRoomEvent>]
[JsonDerivedTypeByName<SendAllInRoomRedisRoomEvent>]
[JsonDerivedTypeByName<SendSpecificUsersRoomRedisRoomEvent>]
[JsonDerivedTypeByName<SendSpecificParticipantRoomRedisRoomEvent>]
public abstract record RedisRoomEvent
{
    public required byte[] Event { get; set; }

    public static RedisRoomEvent Create<T>(T value, Func<byte[], RedisRoomEvent> factory)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        return factory(bytes);
    }

    public abstract T Match<T>(
        Func<ReceivedRedisRoomEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomRedisRoomEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomRedisRoomEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomRedisRoomEvent, T> sendSpecificParticipantRoomRedisRoomEvent
    );
}
