namespace Interview.Domain.Events.Events;

public interface IPayloadBuilder
{
    string? BuildPayload();
}
