using Interview.Domain.Events.Storage;
using NSpecifications;

namespace Interview.Domain.Events.EventProvider;

public sealed class HotRoomEventStorageRoomEventProvider(IHotEventStorage hotEventStorage, Guid id) : IRoomEventProvider
{
    public async Task<IEnumerable<EPStorageEvent>> GetEventsAsync(EPStorageEventRequest request, CancellationToken cancellationToken)
    {
        const int DefaultChunkSize = 500;
        var spec = BuildSpecification(request);
        var changedRooms = await hotEventStorage
            .GetBySpecAsync(spec, DefaultChunkSize, cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        return changedRooms
            .SelectMany(e => e)
            .Select(ToStorageEvent);
    }

    public async Task<EPStorageEvent?> GetLatestEventAsync(EPStorageEventRequest request, CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request);
        var lastCodeEditorState = await hotEventStorage
            .GetLatestBySpecAsync(spec, 1, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
        return lastCodeEditorState?.Select(ToStorageEvent).FirstOrDefault();
    }

    private static EPStorageEvent ToStorageEvent(IStorageEvent e)
    {
        return new EPStorageEvent
        {
            Id = e.Id,
            Payload = e.Payload,
            CreatedAt = e.CreatedAt,
            CreatedById = e.CreatedById,
        };
    }

    private ASpec<IStorageEvent> BuildSpecification(EPStorageEventRequest request)
    {
        var roomId = id;
        var type = request.Type;
        ASpec<IStorageEvent> spec = new Spec<IStorageEvent>(e => e.RoomId == roomId && e.Type == type);
        if (request.From is not null)
        {
            var dateFrom = request.From.Value;
            spec &= new Spec<IStorageEvent>(e => e.CreatedAt >= dateFrom);
        }

        if (request.To is not null)
        {
            var dateTo = request.To.Value;
            spec &= new Spec<IStorageEvent>(e => e.CreatedAt <= dateTo);
        }

        return spec;
    }
}
