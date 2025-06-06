using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomReviews;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Rooms.Service;

public class RoomStatusUpdater(AppDbContext db, IRoomQuestionRepository roomQuestionRepository) : ISelfScopeService
{
    /// <summary>
    /// Close non closed room.
    /// </summary>
    /// <param name="roomId">Room id.</param>
    /// <param name="saveChanges">Save changes.</param>
    /// <param name="cancellationToken">Token.</param>
    /// <returns>Result.</returns>
    public Task CloseWithoutReviewAsync(Guid roomId, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return UpdateRoomStatusAsync(
            roomId,
            saveChanges,
            SERoomStatus.Close,
            async _ =>
            {
                await db.RoomQuestionEvaluation
                    .Include(e => e.RoomQuestion)
                    .Where(e => e.RoomQuestion!.RoomId == roomId && e.State == SERoomQuestionEvaluationState.Draft)
                    .ExecuteUpdateAsync(
                        calls => calls.SetProperty(e => e.State, SERoomQuestionEvaluationState.Rejected),
                        cancellationToken);

                await db.RoomReview
                    .Include(e => e.Participant)
                    .Where(e => e.Participant!.RoomId == roomId && e.State == SERoomReviewState.Open)
                    .ExecuteUpdateAsync(
                        calls => calls.SetProperty(e => e.State, SERoomReviewState.Rejected),
                        cancellationToken);
            },
            cancellationToken);
    }

    public Task CloseAsync(Guid roomId, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return UpdateRoomStatusAsync(roomId, saveChanges, SERoomStatus.Close, null, cancellationToken);
    }

    public Task StartReviewAsync(Guid roomId, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return UpdateRoomStatusAsync(roomId, saveChanges, SERoomStatus.Review, null, cancellationToken);
    }

    private async Task UpdateRoomStatusAsync(
        Guid roomId,
        bool saveChanges,
        SERoomStatus status,
        Func<Room, Task>? additionalAction,
        CancellationToken cancellationToken)
    {
        var currentRoom = await db.Rooms.FirstOrDefaultAsync(e => e.Id == roomId, cancellationToken);
        if (currentRoom is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        if (currentRoom.Status == status)
        {
            throw new UserException($"Room already in '{status}' status");
        }

        await db.RunTransactionAsync(async _ =>
            {
                currentRoom.Status = status;

                await roomQuestionRepository.CloseActiveQuestionAsync(roomId, cancellationToken);
                if (additionalAction is not null)
                {
                    await additionalAction.Invoke(currentRoom);
                }

                if (saveChanges)
                {
                    await db.SaveChangesAsync(cancellationToken);
                }

                return DBNull.Value;
            },
            cancellationToken);
    }
}
