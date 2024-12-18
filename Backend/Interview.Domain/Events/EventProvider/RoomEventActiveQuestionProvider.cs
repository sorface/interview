using System.Runtime.CompilerServices;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.Extensions.Internal;

namespace Interview.Domain.Events.EventProvider;

/// <summary>
/// RoomEventProvider extension.
/// </summary>
public class RoomEventActiveQuestionProvider(IRoomEventProvider roomEventStorage, IEventDeserializer eventDeserializer, ISystemClock clock)
{
    public async IAsyncEnumerable<(DateTime StartActiveDate, DateTime EndActiveDate)> GetActiveQuestionDateAsync(
        Guid questionId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var changedRooms = await roomEventStorage
            .GetEventsAsync(new EPStorageEventRequest { Type = EventType.ChangeRoomQuestionState, From = null, To = null, }, cancellationToken);

        var list = changedRooms
            .Where(e => e.Payload is not null)
            .Select(e => new { Payload = eventDeserializer.Deserialize<RoomQuestionChangeEventPayload>(e.Payload!), CreateAt = e.CreatedAt, })
            .OrderBy(e => e.CreateAt)
            .ToList();
        for (var index = 0; index < list.Count; index++)
        {
            var e = list[index];
            if (e.Payload is null || e.Payload.QuestionId != questionId || e.Payload.NewState != RoomQuestionStateType.Active)
            {
                continue;
            }

            var minDate = e.CreateAt;
            var endActiveDate = list
                .Skip(index + 1)
                .Where(evDetail => evDetail.Payload != null &&
                                   evDetail.Payload.QuestionId != questionId &&
                                   evDetail.Payload.OldState != RoomQuestionStateType.Active &&
                                   evDetail.Payload.NewState == RoomQuestionStateType.Active)
                .Select(evDetail => (DateTime?)evDetail.CreateAt)
                .FirstOrDefault();

            yield return (minDate, endActiveDate ?? clock.UtcNow.LocalDateTime);
        }
    }
}
