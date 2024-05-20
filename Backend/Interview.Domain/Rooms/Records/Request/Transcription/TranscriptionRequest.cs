namespace Interview.Domain.Rooms.Records.Request.Transcription;

public class TranscriptionRequest
{
    public required Guid UserId { get; init; }

    public required Guid RoomId { get; init; }

    public required Dictionary<string, TranscriptionRequestOption> TranscriptionTypeMap { get; init; }
}
