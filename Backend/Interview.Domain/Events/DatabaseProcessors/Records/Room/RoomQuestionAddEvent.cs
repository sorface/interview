using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionAddEvent : RoomEvent<RoomQuestionAddEventPayload>
{
    public RoomQuestionAddEvent(Guid roomId, RoomQuestionAddEventPayload? value)
        : base(roomId, EventType.AddRoomQuestion, value, false)
    {
    }
}
