using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Interview.Domain.Events.Events.Serializers;

namespace Interview.Domain.Events.Events;

public class RoomEvent : RoomEvent<string>
{
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Can't have two files with same name")]
public abstract class RoomEvent<T> : IRoomEvent<T>
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid RoomId { get; init; }

    public string Type { get; init; } = string.Empty;

    public bool Stateful { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public required Guid CreatedById { get; init; }

    public required T? Value { get; init; }

    public string? BuildStringPayload(IEventSerializer serializer)
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
            return payloadBuilder.BuildPayload(serializer);
        }

        return serializer.SerializePayloadAsString(Value);
    }
}
