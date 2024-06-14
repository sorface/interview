using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionPostProcessor : EntityPostProcessor<RoomQuestion>
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomQuestionPostProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    protected override async ValueTask ProcessAddedAsync(RoomQuestion entity, CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionAddEvent(entity.Room!.Id, new AddEventPayload(entity.Question!.Id, entity.State));

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }

    protected override async ValueTask ProcessModifiedAsync(
        RoomQuestion original,
        RoomQuestion current,
        CancellationToken cancellationToken)
    {
        if (original.State == current.State)
        {
            return;
        }

        var @event = new RoomQuestionChangeEvent(
            current.Room!.Id,
            new ChangeEventPayload(current.Question!.Id, original.State, current.State));

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
