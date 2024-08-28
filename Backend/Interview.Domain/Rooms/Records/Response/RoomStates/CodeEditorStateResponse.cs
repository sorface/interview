namespace Interview.Domain.Rooms.Records.Response.RoomStates;

/// <summary>
/// CodeEditorStateResponse.
/// </summary>
public sealed class CodeEditorStateResponse
{
    /// <summary>
    /// Gets or sets content.
    /// </summary>
    public required string? Content { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether enabled.
    /// </summary>
    public required bool Enabled { get; set; }
}
