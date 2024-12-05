using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionAddEvent : RoomEvent<RoomQuestionAddEventPayload>
{
    public RoomQuestionAddEvent()
    {
        Type = EventType.AddRoomQuestion;
    }
}

public sealed record RoomQuestionAddEventPayload(Guid QuestionId, RoomQuestionStateType State);
