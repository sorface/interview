namespace Interview.Domain.PubSub.Events;

public record EventBusRoomEventKey(Guid RoomId) : EventBusKey;
