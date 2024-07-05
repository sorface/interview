using System.Net.WebSockets;
using System.Text;
using Interview.Backend.WebSocket.Events.ConnectionListener;
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

public class WebSocketEventSenderFactory : IEventSenderFactory
{
    private ILogger<WebSocketEventSender> _logger;

    public WebSocketEventSenderFactory(ILogger<WebSocketEventSender> logger)
    {
        _logger = logger;
    }

    public IEventSender Create(IConnectionDetail connectionDetail)
    {
        var webSocket = ((WebSocketConnectDetail)connectionDetail).WebSocket;
        return new WebSocketEventSender(_logger, webSocket);
    }
}
