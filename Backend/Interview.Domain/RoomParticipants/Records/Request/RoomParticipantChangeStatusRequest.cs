namespace Interview.Domain.RoomParticipants.Records.Request
{
    public class RoomParticipantChangeStatusRequest
    {
        public Guid RoomId { get; set; }

        public Guid UserId { get; set; }

        public string UserType { get; set; } = string.Empty;
    }
}
