using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public sealed record RoomQuestionAddEventPayload(Guid QuestionId, RoomQuestionState State);
