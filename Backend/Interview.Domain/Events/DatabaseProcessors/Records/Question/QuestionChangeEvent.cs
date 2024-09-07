using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Question;

public class QuestionChangeEvent : RoomEvent<QuestionChangeEventPayload>
{
    public QuestionChangeEvent(Guid roomId, QuestionChangeEventPayload? value, Guid createdById)
        : base(roomId, EventType.ChangeQuestion, value, false, createdById)
    {
    }
}
