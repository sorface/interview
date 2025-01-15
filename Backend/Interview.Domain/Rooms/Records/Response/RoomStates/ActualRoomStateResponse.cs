using Interview.Domain.Repository;
using Interview.Domain.Rooms.Records.Response.Detail;
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
        CodeEditor = new CodeEditorStateResponse
        {
            Content = room.Configuration == null ? null : room.Configuration.CodeEditorContent,
            Enabled = room.Configuration != null && room.Configuration.CodeEditorEnabled,
        },
        States = room.RoomStates.Select(e => new RoomStateResponse
        {
            Payload = e.Payload,
            Type = e.Type,
        }).ToList(),
        Category = room.Category != null
            ? new RoomCategoryResponse { Id = room.Category.Id, Name = room.Category.Name }
            : null,
    });

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public RoomStateQuestion? ActiveQuestion { get; set; }

    public int LikeCount { get; set; }

    public int DislikeCount { get; set; }

    public required CodeEditorStateResponse CodeEditor { get; set; }

    public required List<RoomStateResponse> States { get; set; }

    public required RoomCategoryResponse? Category { get; set; }
}
