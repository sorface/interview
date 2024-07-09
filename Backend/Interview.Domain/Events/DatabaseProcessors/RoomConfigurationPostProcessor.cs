using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomConfigurations;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomConfigurationPostProcessor : EntityPostProcessor<RoomConfiguration>
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomConfigurationPostProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public override async ValueTask ProcessAddedAsync(RoomConfiguration entity, CancellationToken cancellationToken)
    {
        var @event = CreateEvent(entity, null);

        if (@event is not null)
        {
            await _eventDispatcher.WriteAsync(@event, cancellationToken);
        }
    }

    public override async ValueTask ProcessModifiedAsync(RoomConfiguration original, RoomConfiguration current, CancellationToken cancellationToken)
    {
        var @event = CreateEvent(current, original);
        if (@event is not null)
        {
            await _eventDispatcher.WriteAsync(@event, cancellationToken);
        }
    }

    private static IRoomEvent? CreateEvent(RoomConfiguration current, RoomConfiguration? original)
    {
        if (original is null || original.CodeEditorContent != current.CodeEditorContent)
        {
            return new RoomCodeEditorChangeEvent(current.Id, current.CodeEditorContent);
        }

        return null;
    }
}
