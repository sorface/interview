namespace Interview.Domain.Rooms.RoomParticipants.Records.Request;

public class RoomParticipantGetRequest
{
    public Guid RoomId { get; set; }

    public Guid UserId { get; set; }
}