using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Interview.Backend.Auth;
using Interview.Domain;
using Interview.Infrastructure.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.WebSocket;

[ApiController]
[Route("[controller]")]
public class WebSocketController(WebSocketConnectionHandler connectionHandler, ILogger<WebSocketController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet("/ws")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            throw new UserException("Protocol request error. Use the websocket protocol");
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        logger.LogInformation("WebSocket connection established");

        if (!TryGetUser(out var user))
        {
            return;
        }

        try
        {
            var roomId = await ParseRoomIdAsync(webSocket, CancellationToken.None);
            if (roomId is null)
            {
                return;
            }

            var webSocketConnectHandlerRequest = new WebSocketConnectHandlerRequest
            {
                WebSocket = webSocket,
                User = user,
                RoomId = roomId.Value,
                ServiceProvider = HttpContext.RequestServices,
            };
            await connectionHandler.HandleAsync(webSocketConnectHandlerRequest, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception e)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, e.Message, CancellationToken.None);
            }
            catch
            {
                // ignored
            }
        }
    }

    private async Task<Guid?> ParseRoomIdAsync(System.Net.WebSockets.WebSocket webSocket, CancellationToken ct)
    {
        if (!HttpContext.Request.Query.TryGetValue("roomId", out var roomIdentityString))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Not found roomId", ct);
            return null;
        }

        if (!Guid.TryParse(roomIdentityString, out var roomIdentity))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid room id format", ct);
            return null;
        }

        return roomIdentity;
    }

    private bool TryGetUser([NotNullWhen(true)] out User? user)
    {
        user = HttpContext.User.ToUser();
        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return false;
        }

        return true;
    }
}
