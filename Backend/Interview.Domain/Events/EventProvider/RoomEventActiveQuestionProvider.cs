using System.Runtime.CompilerServices;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.Extensions.Internal;

namespace Interview.Domain.Events.EventProvider;

/// <summary>
/// RoomEventProvider extension.
/// </summary>
public class RoomEventActiveQuestionProvider
{
    private readonly IRoomEventProvider _roomEventStorage;
    private readonly IRoomEventDeserializer _eventDeserializer;
    private readonly ISystemClock _clock;

    public RoomEventActiveQuestionProvider(IRoomEventProvider roomEventStorage, IRoomEventDeserializer eventDeserializer, ISystemClock clock)
    {
        _roomEventStorage = roomEventStorage;
        _eventDeserializer = eventDeserializer;
        _clock = clock;
    }

    public async IAsyncEnumerable<(DateTime StartActiveDate, DateTime EndActiveDate)> GetActiveQuestionDateAsync(
        Guid questionId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var changedRooms = await _roomEventStorage
            .GetEventsAsync(new EPStorageEventRequest { Type = EventType.ChangeRoomQuestionState, From = null, To = null, }, cancellationToken);

        var list = changedRooms
            .Where(e => e.Payload is not null)
            .Select(e => new { Payload = _eventDeserializer.Deserialize<RoomQuestionChangeEventPayload>(e.Payload!), CreateAt = e.CreatedAt, })
            .OrderBy(e => e.CreateAt)
            .ToList();
        foreach (var e in list)
        {
            if (e.Payload is null || e.Payload.QuestionId != questionId || e.Payload.NewState != RoomQuestionStateType.Active)
            {
                continue;
            }

            var minDate = e.CreateAt;
            var endActiveDate = list
                .Where(evDetail => evDetail.Payload is not null && evDetail.Payload.QuestionId == questionId && evDetail.Payload.OldState == RoomQuestionStateType.Active &&
                                   evDetail.CreateAt > minDate)
                .Select(evDetail => (DateTime?)evDetail.CreateAt)
                .FirstOrDefault();

            yield return (minDate, endActiveDate ?? _clock.UtcNow.UtcDateTime);
        }
    }
}
