namespace Interview.Domain.Rooms.Records.Response.Detail;

public class RoomQuestionDetail
{
    public Guid Id { get; set; }

    public string? Value { get; set; }

    public required int Order { get; set; }
}
