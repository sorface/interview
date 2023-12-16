namespace Interview.Domain.Events.Events.Serializers;

public interface IRoomEventSerializer
{
    string SerializeAsString(IRoomEvent? @event);

    string SerializePayloadAsString<T>(T? payload);
}
