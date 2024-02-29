using Interview.Domain.Events.Events;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomQuestionChangeEntityProcessor : IEntityPostProcessor
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomQuestionChangeEntityProcessor(IRoomEventDispatcher eventDispatcher)
    {
            _eventDispatcher = eventDispatcher;
        }

    public async ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken)
    {
            foreach (var entity in entities.OfType<RoomQuestion>())
            {
                await _eventDispatcher.WriteAsync(CreateAddEvent(entity), cancellationToken);
            }
        }

    public async ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken)
    {
            foreach (var (originalEntity, currentEntity) in entities)
            {
                if (originalEntity is not RoomQuestion original ||
                    currentEntity is not RoomQuestion current ||
                    original.State == current.State)
                {
                    continue;
                }

                await _eventDispatcher.WriteAsync(CreateChangeEvent(current, original), cancellationToken);
            }
        }

    private static IRoomEvent CreateChangeEvent(RoomQuestion current, RoomQuestion original)
    {
            return new RoomEvent<ChangeEventPayload>(
                current.Room!.Id,
                EventType.ChangeRoomQuestionState,
                new ChangeEventPayload(current.Question!.Id, original.State!, current.State!),
                false);
        }

    private static IRoomEvent CreateAddEvent(RoomQuestion entity)
    {
            return new RoomEvent<AddEventPayload>(
                entity.Room!.Id,
                EventType.AddRoomQuestion,
                new AddEventPayload(entity.Question!.Id, entity.State!),
                false);
        }

    public sealed record ChangeEventPayload(Guid QuestionId, RoomQuestionState OldState, RoomQuestionState NewState);

    public sealed record AddEventPayload(Guid QuestionId, RoomQuestionState State);
}