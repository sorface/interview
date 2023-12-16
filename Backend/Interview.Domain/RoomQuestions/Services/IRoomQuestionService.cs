using Interview.Domain.RoomQuestions.Records;
using Interview.Domain.RoomQuestions.Records.Response;

namespace Interview.Domain.RoomQuestions.Services;

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
