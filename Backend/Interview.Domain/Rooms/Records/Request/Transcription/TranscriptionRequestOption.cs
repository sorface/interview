using System.ComponentModel.DataAnnotations;

namespace Interview.Domain.Rooms.Records.Request.Transcription;

public class TranscriptionRequestOption
{
    [Required]
    public required string ResponseName { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int Last { get; init; }
}
