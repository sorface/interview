using Interview.Domain.Events.Events.Serializers;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

#pragma warning disable SA1402
public abstract class WebSocketByNameEventHandlerBase<TPayload>(ILogger<WebSocketByNameEventHandlerBase<TPayload>> logger, IEventDeserializer deserializer)
    : WebSocketEventHandlerBase<TPayload>(logger, deserializer)
#pragma warning restore SA1402
{
    protected abstract string SupportType { get; }

    protected override ValueTask<bool> IsSupportTaskAsync(SocketEventDetail detail, CancellationToken cancellationToken)
    {
        var isSupportType = SupportType.Equals(detail.Event.Type, StringComparison.InvariantCultureIgnoreCase);
        return new ValueTask<bool>(isSupportType);
    }

    protected override IEnumerable<KeyValuePair<string, object>> CreateLogScope(SocketEventDetail detail)
    {
        foreach (var keyValuePair in base.CreateLogScope(detail))
        {
            yield return keyValuePair;
        }

        yield return new KeyValuePair<string, object>("SupportType", SupportType);
    }
}

public abstract class WebSocketByNameEventHandlerBase(ILogger<WebSocketByNameEventHandlerBase> logger, IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase<string>(logger, deserializer)
{
    protected override string? ParsePayload(WebSocketEvent @event) => @event.Value;
}
