using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Rooms.Records.Response.RoomStates;

public class ActualRoomStateResponse
{
    public static readonly Mapper<Room, ActualRoomStateResponse> Mapper = new(room => new ActualRoomStateResponse
    {
        Id = room.Id,
        Name = room.Name,
        ActiveQuestion = room.Questions.Select(q => new RoomStateQuestion
        {
            Id = q.Id,
            Value = q.Question!.Value,
            State = q.State!,
        }).FirstOrDefault(q => q.State == RoomQuestionState.Active),
        CodeEditorContent = room.Configuration == null ? null : room.Configuration.CodeEditorContent,
        States = room.RoomStates.Select(e => new RoomStateResponse
        {
            Payload = e.Payload,
            Type = e.Type,
        }).ToList(),
    });

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public RoomStateQuestion? ActiveQuestion { get; set; }

    public int LikeCount { get; set; }

    public int DislikeCount { get; set; }

    public required string? CodeEditorContent { get; set; }

    public required List<RoomStateResponse> States { get; set; }
}
