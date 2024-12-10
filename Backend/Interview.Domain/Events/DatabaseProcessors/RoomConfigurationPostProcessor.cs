using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Users;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomConfigurationPostProcessor(IRoomEventDispatcher eventDispatcher, ICurrentUserAccessor currentUserAccessor) : EntityPostProcessor<RoomConfiguration>
{
    public override async ValueTask ProcessAddedAsync(RoomConfiguration entity, CancellationToken cancellationToken)
    {
        var @event = CreateEvent(entity, null);

        if (@event is not null)
        {
            await eventDispatcher.WriteAsync(@event, cancellationToken);
        }
    }

    public override async ValueTask ProcessModifiedAsync(RoomConfiguration original, RoomConfiguration current, CancellationToken cancellationToken)
    {
        var @event = CreateEvent(current, original);
        if (@event is not null)
        {
            await eventDispatcher.WriteAsync(@event, cancellationToken);
        }
    }

    private IRoomEvent? CreateEvent(RoomConfiguration current, RoomConfiguration? original)
    {
        if (original is null || original.CodeEditorContent != current.CodeEditorContent)
        {
            var payload = new RoomCodeEditorChangeEvent.Payload { Content = current.CodeEditorContent, Source = current.CodeEditorChangeSource, };
            return new RoomCodeEditorChangeEvent
            {
                RoomId = current.Id,
                Value = payload,
                CreatedById = currentUserAccessor.GetUserIdOrThrow(),
            };
        }

        return null;
    }
}
