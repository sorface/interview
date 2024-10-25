namespace Interview.Infrastructure.WebSocket.PubSub.Events;

public record EventBusRoomEventKey(Guid RoomId) : EventBusKey;
