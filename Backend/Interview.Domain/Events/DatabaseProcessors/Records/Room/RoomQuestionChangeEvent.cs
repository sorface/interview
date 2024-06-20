using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionChangeEvent : RoomEvent<RoomQuestionChangeEventPayload>
{
    public RoomQuestionChangeEvent(Guid roomId, RoomQuestionChangeEventPayload? value)
        : base(roomId, EventType.ChangeRoomQuestionState, value, false)
    {
    }
}
