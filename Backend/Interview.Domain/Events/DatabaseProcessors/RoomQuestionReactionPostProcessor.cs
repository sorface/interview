using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Users;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomQuestionReactionPostProcessor(IRoomEventDispatcher eventDispatcher, ICurrentUserAccessor currentUserAccessor) : EntityPostProcessor<RoomQuestionReaction>
{
    public override async ValueTask ProcessAddedAsync(
        RoomQuestionReaction entity,
        CancellationToken cancellationToken)
    {
        var @event = new RoomQuestionReactionEvent
        {
            RoomId = entity.RoomQuestion!.Room!.Id,
            Type = entity.Reaction!.Type.Name,
            Value = new RoomQuestionReactionPayload { Payload = entity.Payload, UserId = entity.Sender!.Id },
            CreatedById = currentUserAccessor.GetUserIdOrThrow(),
        };

        await eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
