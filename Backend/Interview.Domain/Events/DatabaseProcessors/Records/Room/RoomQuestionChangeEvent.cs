using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionChangeEvent : RoomEvent<ChangeEventPayload>
{
    public RoomQuestionChangeEvent(Guid roomId, ChangeEventPayload? value)
        : base(roomId, EventType.ChangeRoomQuestionState, value, false)
    {
    }
}

public sealed record ChangeEventPayload(Guid QuestionId, RoomQuestionState OldState, RoomQuestionState NewState);
