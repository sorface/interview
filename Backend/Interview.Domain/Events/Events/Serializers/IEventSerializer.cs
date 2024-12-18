namespace Interview.Domain.Events.Events.Serializers;

public interface IEventSerializer
{
    string SerializeAsString(IRoomEvent? @event);

    byte[] SerializeToUtf8Bytes<T>(T? payload);

    string SerializePayloadAsString<T>(T? payload);
}

public interface IEventDeserializer
{
    T? Deserialize<T>(string? payload);

    T? Deserialize<T>(byte[]? payload);

    ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken);
}
