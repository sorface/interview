using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Interview.Domain.Events.Sender;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Events.Events;

public class RoomEvent : RoomEvent<string>
{
    public RoomEvent(Guid roomId, string type, string? value, bool stateful)
        : base(roomId, type, value, stateful)
    {
    }

    public RoomEvent(Guid id, Guid roomId, string type, string? value, bool stateful, DateTime createdAt)
        : base(id, roomId, type, value, stateful, createdAt)
    {
    }
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Can't have two files with same name")]
public class RoomEvent<T> : IRoomEvent<T>
{
    public Guid Id { get; }

    public Guid RoomId { get; }

    public string Type { get; }

    public bool Stateful { get; }

    public DateTime CreatedAt { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public virtual IRoomEventFilter Filter { get; init; } = new EmptyRoomEventFilter();

    public T? Value { get; }

    public RoomEvent(Guid id, Guid roomId, string type, T? value, bool stateful, DateTime createdAt)
    {
        Id = id;
        RoomId = roomId;
        Type = type;
        Value = value;
        Stateful = stateful;
        CreatedAt = createdAt;
    }

    public RoomEvent(Guid roomId, string type, T? value, bool stateful)
        : this(Guid.NewGuid(), roomId, type, value, stateful, DateTime.UtcNow)
    {
    }

    public string? BuildStringPayload()
    {
        if (Value is null)
        {
            return null;
        }

        if (typeof(T) == typeof(string))
        {
            return (string)(object)Value;
        }

        if (Value is IPayloadBuilder payloadBuilder)
        {
            return payloadBuilder.BuildPayload();
        }

        return JsonSerializer.Serialize(Value);
    }
}

public interface IRoomEventFilter
{
    bool Satisfy(IConnectionDetail detail);
}

public class EmptyRoomEventFilter : IRoomEventFilter
{
    public bool Satisfy(IConnectionDetail detail) => true;
}
