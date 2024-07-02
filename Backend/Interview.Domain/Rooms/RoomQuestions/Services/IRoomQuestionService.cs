using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services.Update;
using Interview.Domain.ServiceResults.Success;

namespace Interview.Domain.Rooms.RoomQuestions.Services;

public interface IRoomQuestionService : IService
{
    Task<ServiceResult> UpdateAsync(Guid roomId, List<RoomQuestionUpdateRequest> request, CancellationToken cancellationToken = default);

    Task<RoomQuestionDetail> ChangeActiveQuestionAsync(
        RoomQuestionChangeActiveRequest request, CancellationToken cancellationToken = default);

    Task<RoomQuestionDetail> CreateAsync(
        RoomQuestionCreateRequest request,
        CancellationToken cancellationToken);

    Task<List<RoomQuestionResponse>> FindQuestionsAsync(
        RoomQuestionsRequest request, CancellationToken cancellationToken = default);
}
