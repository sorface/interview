using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers
{
#pragma warning disable SA1402
    public abstract class WebSocketEventHandlerBase<TPayload>(ILogger<WebSocketEventHandlerBase<TPayload>> logger) : IWebSocketEventHandler
#pragma warning restore SA1402
    {
        protected readonly ILogger<WebSocketEventHandlerBase<TPayload>> Logger = logger;

        public virtual int Order => 0;

        public async Task<bool> HandleAsync(SocketEventDetail detail, CancellationToken cancellationToken)
        {
            var isSupportType = await IsSupportTaskAsync(detail, cancellationToken);
            if (!isSupportType)
            {
                return false;
            }

            try
            {
                var payload = ParsePayload(detail.Event);
                if (IsValidPayload(payload))
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

        protected virtual bool IsValidPayload(TPayload? payload) => payload is not null;

        protected abstract ValueTask<bool>
            IsSupportTaskAsync(SocketEventDetail detail, CancellationToken cancellationToken);

        protected abstract Task HandleEventAsync(SocketEventDetail detail, TPayload? payload, CancellationToken cancellationToken);

        protected virtual TPayload? ParsePayload(WebSocketEvent @event) => string.IsNullOrEmpty(@event.Value) ? default : JsonSerializer.Deserialize<TPayload>(@event.Value);
    }
}
