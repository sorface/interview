using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Interview.Backend.Auth;
using Interview.Backend.WebSocket.Events;
using Interview.Backend.WebSocket.Events.ConnectionListener;
using Interview.Domain;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.WebSocket;

[ApiController]
[Route("[controller]")]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;
    private readonly IRoomService _roomService;
    private readonly IConnectionListener[] _connectListeners;
    private readonly WebSocketReader _webSocketReader;

    public WebSocketController(
        IRoomService roomService,
        WebSocketReader webSocketReader,
        IEnumerable<IConnectionListener> connectionListeners,
        ILogger<WebSocketController> logger)
    {
        _roomService = roomService;
        _connectListeners = connectionListeners.ToArray();
        _webSocketReader = webSocketReader;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("/ws")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            throw new UserException("Protocol request error. Use the websocket protocol");
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        _logger.LogInformation("WebSocket connection established");

        await ExecuteWebSocket(webSocket, CancellationToken.None);
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task ExecuteWebSocket(System.Net.WebSockets.WebSocket webSocket, CancellationToken ct)
    {
        if (!TryGetUser(out var user))
        {
            return;
        }

        WebSocketConnectDetail? detail = null;
        try
        {
            var roomIdentity = await ParseRoomIdAsync(webSocket, ct);

            if (roomIdentity is null)
            {
                return;
            }

            var (dbRoom, participant) = await _roomService.AddParticipantAsync(roomIdentity.Value, user.Id, ct);

            var participantType = participant.Type.EnumValue;
            detail = new WebSocketConnectDetail(webSocket, dbRoom, user, participantType);
            await HandleListenersSafely(
                nameof(IConnectionListener.OnConnectAsync),
                e => e.OnConnectAsync(detail, ct));

            var waitTask = CreateWaitTask(ct);
            var readerTask = RunEventReaderJob(user, dbRoom, participantType, HttpContext.RequestServices, webSocket, ct);
            await Task.WhenAny(waitTask, readerTask);
            await CloseSafely(webSocket, WebSocketCloseStatus.NormalClosure, string.Empty, ct);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception e)
        {
            await CloseSafely(webSocket, WebSocketCloseStatus.InvalidPayloadData, e.Message, ct);
        }
        finally
        {
            if (detail is not null)
            {
                await HandleListenersSafely(
                    nameof(IConnectionListener.OnDisconnectAsync),
                    e => e.OnDisconnectAsync(detail, CancellationToken.None));
            }
        }

        return;

        static async Task CreateWaitTask(CancellationToken cancellationToken)
        {
            var cst = new TaskCompletionSource<object>();
            await using (cancellationToken.Register(() => cst.TrySetCanceled()))
            {
                await cst.Task;
            }
        }

        static async Task CloseSafely(
            System.Net.WebSockets.WebSocket ws,
            WebSocketCloseStatus status,
            string message,
            CancellationToken cancellationToken)
        {
            try
            {
                await ws.CloseAsync(status, message, cancellationToken);
            }
            catch
            {
                // ignore
            }
        }
    }

    private async Task HandleListenersSafely(string actionName, Func<IConnectionListener, Task> map)
    {
        var tasks = _connectListeners.Select(map);
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "During {Action}", actionName);
        }
    }

    private Task RunEventReaderJob(
        User user,
        Room room,
        EVRoomParticipantType participantType,
        IServiceProvider scopedServiceProvider,
        System.Net.WebSockets.WebSocket webSocket,
        CancellationToken ct)
    {
        return Task.Run(
            () => _webSocketReader.ReadAsync(
                user,
                room,
                participantType,
                scopedServiceProvider,
                webSocket,
                ct),
            ct);
    }

    private async Task<Guid?> ParseRoomIdAsync(System.Net.WebSockets.WebSocket webSocket, CancellationToken ct)
    {
        if (!HttpContext.Request.Query.TryGetValue("roomId", out var roomIdentityString))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid room details", ct);
            return null;
        }

        if (!Guid.TryParse(roomIdentityString, out var roomIdentity))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid room details", ct);
            return null;
        }

        return roomIdentity;
    }

    private bool TryGetUser([NotNullWhen(true)] out User? user)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            user = null;
            return false;
        }

        user = HttpContext.User.ToUser();
        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return false;
        }

        return true;
    }
}
