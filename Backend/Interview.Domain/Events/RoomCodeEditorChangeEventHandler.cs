using Interview.Domain.Database;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events;

public class RoomCodeEditorChangeEventHandler
{
    private readonly AppDbContext _db;
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RoomCodeEditorChangeEventHandler(AppDbContext db, IRoomEventDispatcher eventDispatcher, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _eventDispatcher = eventDispatcher;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var roomConfiguration = await _db.RoomConfiguration.Include(e => e.Room)
            .FirstOrDefaultAsync(e => e.Id == request.RoomId, cancellationToken);
        if (roomConfiguration is null)
        {
            roomConfiguration = new RoomConfiguration
            {
                Id = request.RoomId,
                CodeEditorEnabled = request.Enabled,
                CodeEditorContent = null,
                CodeEditorChangeSource = request.Source,
            };
            await _db.RoomConfiguration.AddAsync(roomConfiguration, cancellationToken);
            if (request.SaveChanges)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            if (roomConfiguration.CodeEditorEnabled == request.Enabled)
            {
                return;
            }

            roomConfiguration.CodeEditorEnabled = request.Enabled;

            if (request.SaveChanges)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        var payload = new RoomCodeEditorEnabledEvent.Payload
        {
            Enabled = request.Enabled,
        };
        var @event = new RoomCodeEditorEnabledEvent(request.RoomId, payload, _currentUserAccessor.GetUserIdOrThrow());
        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }

    public record Request(Guid RoomId, bool Enabled, EVRoomCodeEditorChangeSource Source)
    {
        public bool SaveChanges { get; init; } = true;
    }
}
