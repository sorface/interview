using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionChangeEvent : RoomEvent<RoomQuestionChangeEventPayload>
{
    public RoomQuestionChangeEvent(Guid roomId, RoomQuestionChangeEventPayload? value, Guid createdById)
        : base(roomId, EventType.ChangeRoomQuestionState, value, false, createdById)
    {
    }
}

public sealed record RoomQuestionChangeEventPayload(
    Guid QuestionId,
    RoomQuestionStateType OldState,
    RoomQuestionStateType NewState);
