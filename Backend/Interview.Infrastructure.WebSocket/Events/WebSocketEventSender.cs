using System.Net.WebSockets;
using System.Text;
using Interview.Domain.Events.Sender;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events;

public class WebSocketEventSender(
    ILogger<WebSocketEventSender> logger,
    System.Net.WebSockets.WebSocket webSocket)
    : IEventSender
{
    public async Task SendAsync(ReadOnlyMemory<byte> @event, CancellationToken cancellationToken)
    {
        try
        {
            if (webSocket.ShouldCloseWebSocket())
            {
                return;
            }

            await webSocket.SendAsync(@event, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception e)
        {
            var eventAsStr = Encoding.UTF8.GetString(@event.Span);
            logger.LogError(e, "Send {Event}", eventAsStr);
        }
    }
}
