using Interview.Domain.Questions.QuestionTreeById;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Rooms.Records.Response.RoomStates;

public class ActualRoomStateResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public RoomStateQuestion? ActiveQuestion { get; set; }

    public int LikeCount { get; set; }

    public int DislikeCount { get; set; }

    public required CodeEditorStateResponse CodeEditor { get; set; }

    public required List<RoomStateResponse> States { get; set; }

    public required QuestionTreeByIdResponse? QuestionTree { get; set; }
}
