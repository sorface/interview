using System.Text.Json;

namespace Interview.Backend.WebSocket.Events.Handlers;

public abstract class WebSocketEventHandlerBase<TPayload> : IWebSocketEventHandler
{
    protected readonly ILogger<WebSocketEventHandlerBase<TPayload>> Logger;

    protected WebSocketEventHandlerBase(ILogger<WebSocketEventHandlerBase<TPayload>> logger)
    {
        Logger = logger;
    }

    protected abstract string SupportType { get; }

    public async Task<bool> HandleAsync(SocketEventDetail detail, CancellationToken cancellationToken)
    {
        if (!SupportType.Equals(detail.Event.Type, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        try
        {
            var payload = ParsePayload(detail.Event);
            if (payload is not null)
            {
                await HandleEventAsync(detail, payload, cancellationToken);
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unable to parse payload {Payload}", detail.Event.Value);
        }

        return false;
    }

    protected abstract Task HandleEventAsync(SocketEventDetail detail, TPayload payload, CancellationToken cancellationToken);

    protected virtual TPayload? ParsePayload(WebSocketEvent @event) => @event.Value is null ? default : JsonSerializer.Deserialize<TPayload>(@event.Value);
}

public abstract class WebSocketEventHandlerBase : WebSocketEventHandlerBase<string>
{
    protected WebSocketEventHandlerBase(ILogger<WebSocketEventHandlerBase> logger)
        : base(logger)
    {
    }

    protected override string? ParsePayload(WebSocketEvent @event) => @event.Value;
}
