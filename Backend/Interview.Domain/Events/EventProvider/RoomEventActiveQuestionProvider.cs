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
            .ToList();

        list.Sort((e1, e2) => e1.CreateAt.CompareTo(e2.CreateAt));
        for (var index = 0; index < list.Count; index++)
        {
            var e = list[index];
            if (e.Payload is null || e.Payload.QuestionId != questionId || e.Payload.NewState != RoomQuestionStateType.Active)
            {
                continue;
            }

            var startDate = e.CreateAt;
            var endActiveDate = list
                .Skip(index + 1)
                .Where(evDetail => evDetail.Payload != null &&
                                   evDetail.Payload.QuestionId != questionId &&
                                   evDetail.Payload.OldState != RoomQuestionStateType.Active &&
                                   evDetail.Payload.NewState == RoomQuestionStateType.Active)
                .Select(evDetail => (DateTime?)evDetail.CreateAt)
                .FirstOrDefault();

            var endDate = CalculateEndDate(startDate, endActiveDate);
            yield return (startDate, endDate);
        }
    }

    private DateTime CalculateEndDate(DateTime startDate, DateTime? endDate)
    {
        if (endDate is not null)
        {
            return endDate.Value;
        }

        return startDate.Kind == DateTimeKind.Local
            ? clock.UtcNow.LocalDateTime
            : clock.UtcNow.UtcDateTime;
    }
}
