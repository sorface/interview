using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionAddEvent : RoomEvent<RoomQuestionAddEventPayload>
{
    public RoomQuestionAddEvent(Guid roomId, RoomQuestionAddEventPayload? value)
        : base(roomId, EventType.AddRoomQuestion, value, false)
    {
    }
}

public sealed record RoomQuestionAddEventPayload(Guid QuestionId, RoomQuestionState State);
