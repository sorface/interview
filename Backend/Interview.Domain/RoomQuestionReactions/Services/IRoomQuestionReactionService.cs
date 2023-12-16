using Interview.Domain.RoomQuestionReactions.Records;
using Interview.Domain.RoomQuestionReactions.Records.Response;

namespace Interview.Domain.RoomQuestionReactions
{
    public interface IRoomQuestionReactionService : IService
    {
        Task<RoomQuestionReactionDetail> CreateAsync(
            RoomQuestionReactionCreateRequest request,
            Guid userId,
            CancellationToken cancellationToken);
    }
}
