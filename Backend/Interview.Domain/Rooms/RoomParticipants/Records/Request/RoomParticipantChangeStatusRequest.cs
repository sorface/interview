namespace Interview.Domain.Rooms.RoomParticipants.Records.Request;

public class RoomParticipantChangeStatusRequest
{
    public Guid RoomId { get; set; }

    public Guid UserId { get; set; }

    public EVRoomParticipantType UserType { get; set; }
}
