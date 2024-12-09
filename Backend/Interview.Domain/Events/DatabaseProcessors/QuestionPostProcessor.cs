using Interview.Domain.Connections;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Questions;
using Interview.Domain.Users;

namespace Interview.Domain.Events.DatabaseProcessors;

public class QuestionPostProcessor(IRoomEventDispatcher eventDispatcher, IActiveRoomSource connectUserSource, ICurrentUserAccessor currentUserAccessor)
    : EntityPostProcessor<Question>
{
    public override async ValueTask ProcessModifiedAsync(
        Question original,
        Question current,
        CancellationToken cancellationToken)
    {
        foreach (var roomId in connectUserSource.ActiveRooms)
        {
            var questionEventPayload = new QuestionChangeEventPayload(current.Id, original.Value, current.Value);

            var @event = new QuestionChangeEvent
            {
                RoomId = roomId,
                Value = questionEventPayload,
                CreatedById = currentUserAccessor.GetUserIdOrThrow(),
            };

            await eventDispatcher.WriteAsync(@event, cancellationToken);
        }
    }
}
