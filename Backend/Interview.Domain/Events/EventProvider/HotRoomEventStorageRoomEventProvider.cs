using Interview.Domain.Events.Storage;
using NSpecifications;

namespace Interview.Domain.Events.EventProvider;

public sealed class HotRoomEventStorageRoomEventProvider : IRoomEventProvider
{
    private readonly IHotEventStorage _hotEventStorage;
    private readonly Guid _roomId;

    public HotRoomEventStorageRoomEventProvider(IHotEventStorage hotEventStorage, Guid roomId)
    {
        _hotEventStorage = hotEventStorage;
        _roomId = roomId;
    }

    public async Task<IEnumerable<EPStorageEvent>> GetEventsAsync(EPStorageEventRequest request, CancellationToken cancellationToken)
    {
        const int DefaultChunkSize = 500;
        var spec = BuildSpecification(request);
        var changedRooms = await _hotEventStorage
            .GetBySpecAsync(spec, DefaultChunkSize, cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        return changedRooms
            .SelectMany(e => e)
            .Select(ToStorageEvent);
    }

    public async Task<EPStorageEvent?> GetLatestEventAsync(EPStorageEventRequest request, CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request);
        var lastCodeEditorState = await _hotEventStorage
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
        ASpec<IStorageEvent> spec = new Spec<IStorageEvent>(e => e.RoomId == _roomId && e.Type == request.Type);
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
