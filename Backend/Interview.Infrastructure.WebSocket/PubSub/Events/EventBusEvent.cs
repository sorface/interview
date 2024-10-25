using System.Text.Json;

namespace Interview.Infrastructure.WebSocket.PubSub.Events;

[JsonDerivedTypeByName<ReceivedEventBusEvent>]
[JsonDerivedTypeByName<SendAllInRoomEventBusEvent>]
[JsonDerivedTypeByName<SendSpecificUsersRoomEventBusEvent>]
[JsonDerivedTypeByName<SendSpecificParticipantRoomEventBusEvent>]
public abstract record EventBusEvent
{
    public required byte[] Event { get; set; }
    
    public static byte[] ToBytes<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value);

    public abstract T Match<T>(
        Func<ReceivedEventBusEvent, T> receiveRedisRoomEvent,
        Func<SendAllInRoomEventBusEvent, T> sendAllInRoomRedisRoomEvent,
        Func<SendSpecificUsersRoomEventBusEvent, T> sendSpecificUsersRoomRedisRoomEvent,
        Func<SendSpecificParticipantRoomEventBusEvent, T> sendSpecificParticipantRoomRedisRoomEvent
    );
}
