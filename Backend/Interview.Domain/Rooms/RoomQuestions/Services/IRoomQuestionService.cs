using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;

namespace Interview.Domain.Rooms.RoomQuestions.Services;

public interface IRoomQuestionService : IService
{
    Task<RoomQuestionDetail> ChangeActiveQuestionAsync(
        RoomQuestionChangeActiveRequest request, CancellationToken cancellationToken = default);

    Task<RoomQuestionDetail> CreateAsync(
        RoomQuestionCreateRequest request,
        CancellationToken cancellationToken);

    Task<List<Guid>> FindGuidsAsync(
        RoomQuestionsRequest request, CancellationToken cancellationToken = default);
}
