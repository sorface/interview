using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionAddEvent : RoomEvent<RoomQuestionAddEventPayload>
{
    public RoomQuestionAddEvent(Guid roomId, RoomQuestionAddEventPayload? value, Guid createdById)
        : base(roomId, EventType.AddRoomQuestion, value, false, createdById)
    {
    }
}

public sealed record RoomQuestionAddEventPayload(Guid QuestionId, RoomQuestionState State);
