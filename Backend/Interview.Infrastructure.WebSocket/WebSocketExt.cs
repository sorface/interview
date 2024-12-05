using System.Net.WebSockets;

namespace Interview.Infrastructure.WebSocket;

public static class WebSocketExt
{
    public static bool ShouldCloseWebSocket(this System.Net.WebSockets.WebSocket entry)
    {
        return entry.State is WebSocketState.Aborted or WebSocketState.Closed or WebSocketState.CloseReceived or WebSocketState.CloseSent ||
               entry.CloseStatus.HasValue;
    }
}
