using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.WebSocket.Events;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Interview.Infrastructure.WebSocket.PubSub;
using Interview.Infrastructure.WebSocket.PubSub.Events;
using Interview.Infrastructure.WebSocket.PubSub.Factory;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket;

public sealed class WebSocketConnectHandlerRequest
{
    public required System.Net.WebSockets.WebSocket WebSocket { get; set; }

    public required User User { get; set; }

    public required Guid RoomId { get; set; }

    public required IServiceProvider ServiceProvider { get; set; }
}

public class WebSocketConnectionHandler
{
    private readonly ILogger<WebSocketConnectionHandler> _logger;
    private readonly IRoomService _roomService;
    private readonly IConnectionListener[] _connectListeners;
    private readonly WebSocketReader _webSocketReader;
    private readonly IEventBusPublisherFactory _publisherFactory;
    private readonly IEventBusSubscriberFactory _subscriberFactory;

    public WebSocketConnectionHandler(
        IRoomService roomService,
        WebSocketReader webSocketReader,
        IEnumerable<IConnectionListener> connectionListeners,
        ILogger<WebSocketConnectionHandler> logger,
        IEventBusPublisherFactory publisherFactory,
        IEventBusSubscriberFactory subscriberFactory)
    {
        _roomService = roomService;
        _connectListeners = connectionListeners.ToArray();
        _webSocketReader = webSocketReader;
        _logger = logger;
        _publisherFactory = publisherFactory;
        _subscriberFactory = subscriberFactory;
    }

    public async Task HandleAsync(WebSocketConnectHandlerRequest request, CancellationToken ct)
    {
        WebSocketConnectDetail? detail = null;
        try
        {
            detail = await HandleAsyncCore(request, ct);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception e)
        {
            await CloseSafely(request.WebSocket, WebSocketCloseStatus.InvalidPayloadData, e.Message, ct);
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
    }

    private async Task<WebSocketConnectDetail?> HandleAsyncCore(WebSocketConnectHandlerRequest request, CancellationToken ct)
    {
        var (dbRoom, participant) = await _roomService.AddParticipantAsync(request.RoomId, request.User.Id, ct);

        var participantType = participant.Type.EnumValue;
        var detail = new WebSocketConnectDetail(request.WebSocket, dbRoom, request.User, participantType);

        var subscriber = await _subscriberFactory.CreateAsync(ct);
        
        // subscriber.SubscribeAsync(new EventBusRoomEventKey(request.RoomId))
        await HandleListenersSafely(
            nameof(IConnectionListener.OnConnectAsync),
            e => e.OnConnectAsync(detail, ct));

        var waitTask = CreateWaitTask(ct);
        var readerTask = Task.Run(
            () => _webSocketReader.ReadAsync(
                request.User,
                dbRoom,
                participantType,
                request.ServiceProvider,
                request.WebSocket,
                ct),
            ct);
        await Task.WhenAny(waitTask, readerTask);
        await CloseSafely(request.WebSocket, WebSocketCloseStatus.NormalClosure, string.Empty, ct);
        return detail;
    }

    private static async Task CreateWaitTask(CancellationToken cancellationToken)
    {
        var cst = new TaskCompletionSource<object>();
        await using (cancellationToken.Register(() => cst.TrySetCanceled()))
        {
            await cst.Task;
        }
    }

    private static async Task CloseSafely(
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
}
