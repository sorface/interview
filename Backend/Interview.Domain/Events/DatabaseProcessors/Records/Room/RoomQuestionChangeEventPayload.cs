using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public sealed record RoomQuestionChangeEventPayload(
    Guid QuestionId,
    RoomQuestionState OldState,
    RoomQuestionState NewState);
