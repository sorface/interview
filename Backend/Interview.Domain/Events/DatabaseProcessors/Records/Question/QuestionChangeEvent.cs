using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Question;

public class QuestionChangeEvent : RoomEvent<QuestionChangeEventPayload>
{
    public QuestionChangeEvent()
    {
        Type = EventType.ChangeQuestion;
    }
}
