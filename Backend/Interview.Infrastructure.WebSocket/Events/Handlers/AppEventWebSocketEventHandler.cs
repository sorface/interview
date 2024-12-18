using Interview.Domain.Database;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public sealed class AppEventWebSocketEventHandler(ILogger<WebSocketEventHandlerBase<Dictionary<string, object>>> logger, IEventDeserializer deserializer)
    : WebSocketEventHandlerBase<Dictionary<string, object>>(logger, deserializer)
{
    public override int Order => int.MaxValue;

    protected override async ValueTask<bool> IsSupportTaskAsync(SocketEventDetail detail, CancellationToken cancellationToken)
    {
        var db = detail.ScopedServiceProvider.GetRequiredService<AppDbContext>();
        var result = await db.AppEvent.AsNoTracking()
            .AnyAsync(e => e.Type == detail.Event.Type, cancellationToken: cancellationToken);
        return result;
    }

    protected override bool IsValidPayload(Dictionary<string, object>? payload) => true;

    protected override Task HandleEventAsync(SocketEventDetail detail, Dictionary<string, object>? payload, CancellationToken cancellationToken)
    {
        var roomService = detail.ScopedServiceProvider.GetRequiredService<IRoomService>();
        var request = new RoomEventRequest(detail.RoomId, detail.UserId, detail.Event.Type, payload);
        return roomService.SendEventRequestAsync(request, cancellationToken);
    }
}
