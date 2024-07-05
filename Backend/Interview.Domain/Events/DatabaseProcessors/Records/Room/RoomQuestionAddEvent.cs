using Interview.Domain.Events.Events;
using Interview.Domain.Events.Sender;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public sealed class RoomQuestionAddEvent : RoomEvent<RoomQuestionAddEventPayload>
{
    public RoomQuestionAddEvent(Guid roomId, RoomQuestionAddEventPayload? value)
        : base(roomId, EventType.AddRoomQuestion, value, false)
    {
        Filter = new AllAggregateRoomEventFilter
        {
            new OrAggregateRoomEventFilter
            {
                new SpecificParticipantOnlyFilter(EVRoomParticipantType.Expert),
                new SpecificParticipantOnlyFilter(EVRoomParticipantType.Examinee),
            },
        };
    }

    private class SpecificParticipantOnlyFilter : IRoomEventFilter
    {
        private EVRoomParticipantType _participantType;

        public SpecificParticipantOnlyFilter(EVRoomParticipantType participantType)
        {
            _participantType = participantType;
        }

        public bool Satisfy(IConnectionDetail detail)
        {
            return detail.ParticipantType == _participantType;
        }
    }
}

public sealed record RoomQuestionAddEventPayload(Guid QuestionId, RoomQuestionState State);

public class AllAggregateRoomEventFilter : List<IRoomEventFilter>, IRoomEventFilter
{
    public bool Satisfy(IConnectionDetail detail)
    {
        return this.All(e => e.Satisfy(detail));
    }
}

public class OrAggregateRoomEventFilter : List<IRoomEventFilter>, IRoomEventFilter
{
    public bool Satisfy(IConnectionDetail detail)
    {
        return this.Any(e => e.Satisfy(detail));
    }
}
