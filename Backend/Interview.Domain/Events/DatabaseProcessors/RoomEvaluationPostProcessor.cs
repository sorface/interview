using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Users;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomEvaluationPostProcessor : EntityPostProcessor<RoomQuestionEvaluation>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly AppDbContext _databaseContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RoomEvaluationPostProcessor(IRoomEventDispatcher eventDispatcher, AppDbContext databaseContext, ICurrentUserAccessor currentUserAccessor)
    {
        _eventDispatcher = eventDispatcher;
        _databaseContext = databaseContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public override async ValueTask ProcessAddedAsync(RoomQuestionEvaluation entity, CancellationToken cancellationToken)
    {
        await HandleEvent(entity, cancellationToken, (roomId, questionId) =>
        {
            var evaluationPayload = new RoomEvaluationAddEventPayload(questionId);
            return new RoomEvaluationAddEvent(roomId, evaluationPayload, _currentUserAccessor.GetUserIdOrThrow());
        });
    }

    public override async ValueTask ProcessModifiedAsync(RoomQuestionEvaluation original, RoomQuestionEvaluation current, CancellationToken cancellationToken)
    {
        await HandleEvent(current, cancellationToken, (roomId, questionId) =>
        {
            var evaluationPayload = new RoomEvaluationChangeEventPayload(questionId);
            return new RoomEvaluationChangeEvent(roomId, evaluationPayload, _currentUserAccessor.GetUserIdOrThrow());
        });
    }

    private async ValueTask HandleEvent<T>(RoomQuestionEvaluation entity, CancellationToken cancellationToken, Func<Guid, Guid, T> funcEventMapper)
        where T : IRoomEvent
    {
        if (entity.RoomQuestion is null)
        {
            await _databaseContext.Entry(entity).Reference(e => e.RoomQuestion).LoadAsync(cancellationToken);
        }

        if (entity.RoomQuestion is null)
        {
            return;
        }

        var @event = funcEventMapper.Invoke(entity.RoomQuestion.RoomId, entity.RoomQuestion.QuestionId);
        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
