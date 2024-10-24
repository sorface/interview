namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record RedisRoomEventKey(Guid RoomId) : PubSubKey;
