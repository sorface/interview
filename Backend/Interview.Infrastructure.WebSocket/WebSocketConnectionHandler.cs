using System.Net.WebSockets;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.WebSocket.Events;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket;

/// <summary>
/// Web socket connection handler.
/// </summary>
public class WebSocketConnectionHandler(
    WebSocketReader webSocketReader,
    IEnumerable<IConnectionListener> connectionListeners,
    ILogger<WebSocketConnectionHandler> logger,
    AppDbContext dbContext)
{
    private readonly IConnectionListener[] _connectListeners = connectionListeners.ToArray();

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
            throw;
        }
        catch (Exception e)
        {
            logger.LogDebug(e, "Exception inside HandleAsyncCore");
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

    private async Task<WebSocketConnectDetail?> HandleAsyncCore(WebSocketConnectHandlerRequest request, CancellationToken ct)
    {
        var roomParticipantDetail = await dbContext.RoomParticipants
            .Include(e => e.Room)
            .Where(e => e.RoomId == request.RoomId && e.UserId == request.User.Id)
            .Select(e => new { e.Id, e.Type, e.Room })
            .FirstOrDefaultAsync(ct);
        if (roomParticipantDetail is null)
        {
            throw new AccessDeniedException("No room participants found");
        }

        using var scope = CreateLoggingScope(request, roomParticipantDetail.Room, roomParticipantDetail.Id, roomParticipantDetail.Type);
        logger.LogInformation("Connect to room");

        try
        {
            var participantType = roomParticipantDetail.Type.EnumValue;
            var detail = new WebSocketConnectDetail(request.WebSocket, roomParticipantDetail.Room, request.User, participantType);

            await HandleListenersSafely(
                nameof(IConnectionListener.OnConnectAsync),
                e => e.OnConnectAsync(detail, ct));

            var waitTask = CreateWaitTask(ct);
            var scopeFactory = request.ServiceProvider.GetRequiredService<CurrentUserServiceScopeFactory>();
            var readerTask = Task.Run(() => webSocketReader.ReadAsync(request.User, roomParticipantDetail.Room, participantType, scopeFactory, request.WebSocket, ct), ct);
            await Task.WhenAny(waitTask, readerTask);
            await CloseSafely(request.WebSocket, WebSocketCloseStatus.NormalClosure, string.Empty, ct);
            return detail;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to connect to room {CloseStatus} {WebSocketState}", request.WebSocket.CloseStatus, request.WebSocket.State);
            throw;
        }
        finally
        {
            logger.LogInformation("Disconnect from room");
        }
    }

    private IDisposable? CreateLoggingScope(WebSocketConnectHandlerRequest request, Room dbRoom, Guid participantId, SERoomParticipantType type)
    {
        var keyValuePairs = new List<KeyValuePair<string, object>>
        {
            new("RoomId", dbRoom.Id),
            new("RoomName", dbRoom.Name),
            new("ParticipantId", participantId),
            new("ParticipantType", type.Name),
            new("UserId", request.User.Id),
        };
        if (request.User?.Nickname is not null)
        {
            keyValuePairs.Add(new KeyValuePair<string, object>("Nickname", request.User.Nickname));
        }

        return logger.BeginScope(keyValuePairs);
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
            logger.LogError(e, "During {Action}", actionName);
        }
    }
}

public sealed class WebSocketConnectHandlerRequest
{
    public required System.Net.WebSockets.WebSocket WebSocket { get; set; }

    public required User User { get; set; }

    public required Guid RoomId { get; set; }

    public required IServiceProvider ServiceProvider { get; set; }
}
