using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

#pragma warning disable SA1402
public abstract class WebSocketByNameEventHandlerBase<TPayload>(ILogger<WebSocketByNameEventHandlerBase<TPayload>> logger) : WebSocketEventHandlerBase<TPayload>(logger)
#pragma warning restore SA1402
{
    protected abstract string SupportType { get; }

    protected override ValueTask<bool> IsSupportTaskAsync(SocketEventDetail detail, CancellationToken cancellationToken)
    {
        var isSupportType = SupportType.Equals(detail.Event.Type, StringComparison.InvariantCultureIgnoreCase);
        return new ValueTask<bool>(isSupportType);
    }
}

public abstract class WebSocketByNameEventHandlerBase(ILogger<WebSocketByNameEventHandlerBase> logger) : WebSocketByNameEventHandlerBase<string>(logger)
{
    protected override string? ParsePayload(WebSocketEvent @event) => @event.Value;
}
