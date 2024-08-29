using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionChangeEvent : RoomEvent<RoomQuestionChangeEventPayload>
{
    public RoomQuestionChangeEvent(Guid roomId, RoomQuestionChangeEventPayload? value)
        : base(roomId, EventType.ChangeRoomQuestionState, value, false)
    {
    }
}

public sealed record RoomQuestionChangeEventPayload(
    Guid QuestionId,
    RoomQuestionStateType OldState,
    RoomQuestionStateType NewState);
