namespace Interview.Domain.RoomParticipants.Records.Request
{
    public class RoomParticipantCreateRequest
    {
        public Guid RoomId { get; set; }

        public Guid UserId { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}
