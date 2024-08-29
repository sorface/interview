using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Users;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionReactionPostProcessor : EntityPostProcessor<RoomQuestionReaction>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RoomQuestionReactionPostProcessor(IRoomEventDispatcher eventDispatcher, ICurrentUserAccessor currentUserAccessor)
    {
        _eventDispatcher = eventDispatcher;
        _currentUserAccessor = currentUserAccessor;
    }

    public override async ValueTask ProcessAddedAsync(
        RoomQuestionReaction entity,
        CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionReactionEvent(
            entity.RoomQuestion!.Room!.Id,
            entity.Reaction!.Type.Name,
            new RoomQuestionReactionPayload(entity.Sender!.Id, entity.Payload),
            _currentUserAccessor.GetUserIdOrThrow());

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
