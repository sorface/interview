using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Interview.Domain.Events.Events;

public class RoomEvent : RoomEvent<string>
{
    public RoomEvent(Guid roomId, string type, string? value, bool stateful, Guid createdById)
        : base(roomId, type, value, stateful, createdById)
    {
    }

    public RoomEvent(Guid id, Guid roomId, string type, string? value, bool stateful, DateTime createdAt, Guid createdById)
        : base(id, roomId, type, value, stateful, createdAt, createdById)
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

    public Guid CreatedById { get; }

    public T? Value { get; }

    public RoomEvent(Guid id, Guid roomId, string type, T? value, bool stateful, DateTime createdAt, Guid createdById)
    {
        Id = id;
        RoomId = roomId;
        Type = type;
        Value = value;
        Stateful = stateful;
        CreatedAt = createdAt;
        CreatedById = createdById;
    }

    public RoomEvent(Guid roomId, string type, T? value, bool stateful, Guid createdById)
        : this(Guid.NewGuid(), roomId, type, value, stateful, DateTime.UtcNow, createdById)
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
