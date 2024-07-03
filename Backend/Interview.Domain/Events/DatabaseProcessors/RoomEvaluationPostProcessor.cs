using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestionEvaluations;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomEvaluationPostProcessor : EntityPostProcessor<RoomQuestionEvaluation>
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    private readonly AppDbContext _databaseContext;

    public RoomEvaluationPostProcessor(IRoomEventDispatcher eventDispatcher, AppDbContext databaseContext)
    {
        _eventDispatcher = eventDispatcher;
        _databaseContext = databaseContext;
    }

    public override async ValueTask ProcessAddedAsync(RoomQuestionEvaluation entity, CancellationToken cancellationToken)
    {
        await HandleEvent(entity, cancellationToken, (roomId, questionId) =>
        {
            var evaluationPayload = new RoomEvaluationAddEventPayload(questionId);
            return new RoomEvaluationAddEvent(roomId, evaluationPayload);
        });
    }

    public override async ValueTask ProcessModifiedAsync(RoomQuestionEvaluation original, RoomQuestionEvaluation current, CancellationToken cancellationToken)
    {
        await HandleEvent(current, cancellationToken, (roomId, questionId) =>
        {
            var evaluationPayload = new RoomEvaluationChangeEventPayload(questionId);
            return new RoomEvaluationChangeEvent(roomId, evaluationPayload);
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

        if (entity.RoomQuestion.Question is null)
        {
            await _databaseContext.Entry(entity.RoomQuestion).Reference(roomQuestion => roomQuestion.Question).LoadAsync(cancellationToken);
        }

        if (entity.RoomQuestion.Question is null)
        {
            return;
        }

        if (entity.RoomQuestion.Room is null)
        {
            await _databaseContext.Entry(entity.RoomQuestion).Reference(roomQuestion => roomQuestion.Room).LoadAsync(cancellationToken);
        }

        if (entity.RoomQuestion.Room is null || entity.RoomQuestion.Question is null)
        {
            return;
        }

        var @event = funcEventMapper.Invoke(entity.RoomQuestion.RoomId, entity.RoomQuestion.QuestionId);

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
