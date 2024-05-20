namespace Interview.Domain.Rooms.Records.Response.Detail;

public class RoomInviteDetail
{
    public Guid ParticipantId { get; set; }

    public Guid RoomId { get; set; }

    public string? ParticipantType { get; set; }
}
