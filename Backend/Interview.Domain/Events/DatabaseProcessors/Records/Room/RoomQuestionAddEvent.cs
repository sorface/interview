using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionAddEvent : RoomEvent<AddEventPayload>
{
    public RoomQuestionAddEvent(Guid roomId, AddEventPayload? value)
        : base(roomId, EventType.AddRoomQuestion, value, false)
    {
    }
}

public sealed record AddEventPayload(Guid QuestionId, RoomQuestionState State);
