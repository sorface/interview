using System.Net.WebSockets;
using System.Text;
using Interview.Domain.Events.Sender;

namespace Interview.Backend.WebSocket.Events;

public class WebSocketEventSender : IEventSender
{
    private readonly ILogger<WebSocketEventSender> _logger;
    private readonly System.Net.WebSockets.WebSocket _webSocket;

    public WebSocketEventSender(
        ILogger<WebSocketEventSender> logger,
        System.Net.WebSockets.WebSocket webSocket)
    {
        _logger = logger;
        _webSocket = webSocket;
    }

    public async Task SendAsync(ReadOnlyMemory<byte> @event, CancellationToken cancellationToken)
    {
        try
        {
            if (_webSocket.ShouldCloseWebSocket())
            {
                return;
            }

            await _webSocket.SendAsync(@event, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception e)
        {
            var eventAsStr = Encoding.UTF8.GetString(@event.Span);
            _logger.LogError(e, "Send {Event}", eventAsStr);
        }
    }
}
