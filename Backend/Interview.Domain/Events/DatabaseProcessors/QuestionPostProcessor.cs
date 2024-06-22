using Interview.Domain.Connections;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Questions;

namespace Interview.Domain.Events.DatabaseProcessors;

public class QuestionPostProcessor : EntityPostProcessor<Question>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly IActiveRoomSource _connectUserSource;

    public QuestionPostProcessor(IRoomEventDispatcher eventDispatcher, IActiveRoomSource connectUserSource)
    {
        _eventDispatcher = eventDispatcher;
        _connectUserSource = connectUserSource;
    }

    public override async ValueTask ProcessModifiedAsync(
        Question original,
        Question current,
        CancellationToken cancellationToken)
    {
        foreach (var roomId in _connectUserSource.ActiveRooms)
        {
            var questionEventPayload = new QuestionChangeEventPayload(current.Id, original.Value, current.Value);

            var @event = new QuestionChangeEvent(roomId, questionEventPayload);

            await _eventDispatcher.WriteAsync(@event, cancellationToken);
        }
    }
}
