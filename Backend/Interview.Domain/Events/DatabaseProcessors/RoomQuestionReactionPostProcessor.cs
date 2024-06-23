using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomQuestionReactions;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionReactionPostProcessor : EntityPostProcessor<RoomQuestionReaction>
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomQuestionReactionPostProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public override async ValueTask ProcessAddedAsync(
        RoomQuestionReaction entity,
        CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionReactionEvent(
            entity.RoomQuestion!.Room!.Id,
            entity.Reaction!.Type.Name,
            new RoomQuestionReactionPayload(entity.Sender!.Id, entity.Payload));

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
