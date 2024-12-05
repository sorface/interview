using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionChangeEvent : RoomEvent<RoomQuestionChangeEventPayload>
{
    public RoomQuestionChangeEvent()
    {
        Type = EventType.ChangeRoomQuestionState;
    }
}

public sealed record RoomQuestionChangeEventPayload(
    Guid QuestionId,
    RoomQuestionStateType OldState,
    RoomQuestionStateType NewState);
