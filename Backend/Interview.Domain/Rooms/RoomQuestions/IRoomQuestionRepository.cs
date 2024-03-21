using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomQuestions;

public interface IRoomQuestionRepository : IRepository<RoomQuestion>
{
    /// <summary>
    /// Close active room question.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="cancellationToken">Token.</param>
    /// <returns>Active question.</returns>
    public Task<bool> CloseActiveQuestionAsync(Guid roomId, CancellationToken cancellationToken);

    public Task<RoomQuestion?> FindFirstByQuestionIdAndRoomIdAsync(Guid questionId, Guid roomId, CancellationToken cancellationToken);

    public Task<RoomQuestion?> FindFirstByRoomAndStateAsync(Guid roomId, RoomQuestionState roomQuestionState, CancellationToken cancellationToken = default);
}
