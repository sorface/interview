using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.PubSub;
using Interview.Domain.Rooms.Records.Request;

namespace Interview.Domain.Events.Events;

[JsonDerivedTypeByName<QuestionChangeEvent>]
[JsonDerivedTypeByName<RoomCodeEditorEnabledEvent>]
[JsonDerivedTypeByName<RoomChangeStatusEvent>]
[JsonDerivedTypeByName<RoomCodeEditorChangeEvent>]
[JsonDerivedTypeByName<RoomEvaluationAddEvent>]
[JsonDerivedTypeByName<RoomEvaluationChangeEvent>]
[JsonDerivedTypeByName<RoomQuestionAddEvent>]
[JsonDerivedTypeByName<RoomQuestionChangeEvent>]
[JsonDerivedTypeByName<RoomQuestionReactionEvent>]
[JsonDerivedTypeByName<RoomTimerStartEvent>]
[JsonDerivedTypeByName<RoomEvent>]
[JsonDerivedTypeByName<RoomUserEvent>]
[JsonDerivedTypeByName<RoomMessageEvent>]
public interface IRoomEvent
{
    Guid Id { get; }

    Guid RoomId { get; }

    string Type { get; }

    bool Stateful { get; }

    DateTime CreatedAt { get; }

    Guid CreatedById { get; }

    string? BuildStringPayload(IEventSerializer serializer);
}

public interface IRoomEvent<out T> : IRoomEvent
{
    T? Value { get; }
}
