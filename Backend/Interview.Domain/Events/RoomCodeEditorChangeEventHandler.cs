using Interview.Domain.Database;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events;

public class RoomCodeEditorChangeEventHandler(AppDbContext db, IRoomEventDispatcher eventDispatcher, ICurrentUserAccessor currentUserAccessor)
{
    public async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var roomConfiguration = await db.RoomConfiguration.Include(e => e.Room)
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
            await db.RoomConfiguration.AddAsync(roomConfiguration, cancellationToken);
            if (request.SaveChanges)
            {
                await db.SaveChangesAsync(cancellationToken);
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
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        var payload = new RoomCodeEditorEnabledEvent.Payload
        {
            Enabled = request.Enabled,
        };
        var @event = new RoomCodeEditorEnabledEvent
        {
            RoomId = request.RoomId,
            Value = payload,
            CreatedById = currentUserAccessor.GetUserIdOrThrow(),
        };
        await eventDispatcher.WriteAsync(@event, cancellationToken);
    }

    public record Request(Guid RoomId, bool Enabled, EVRoomCodeEditorChangeSource Source)
    {
        public bool SaveChanges { get; init; } = true;
    }
}
