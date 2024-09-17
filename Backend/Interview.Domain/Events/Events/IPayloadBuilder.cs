using Interview.Domain.Events.Events.Serializers;

namespace Interview.Domain.Events.Events;

public interface IPayloadBuilder
{
    string? BuildPayload(IRoomEventSerializer serializer);
}
