using Interview.Domain.Rooms.RoomQuestionReactions.Records;
using Interview.Domain.Rooms.RoomQuestionReactions.Records.Response;

namespace Interview.Domain.Rooms.RoomQuestionReactions.Services;

public interface IRoomQuestionReactionService : IService
{
    Task<RoomQuestionReactionDetail> CreateAsync(
        RoomQuestionReactionCreateRequest request,
        Guid userId,
        CancellationToken cancellationToken);
}
