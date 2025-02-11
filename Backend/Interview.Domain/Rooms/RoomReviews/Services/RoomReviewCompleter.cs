using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.Service;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Rooms.RoomReviews.Services;

public class RoomReviewCompleter(
    AppDbContext db,
    IRoomParticipantRepository roomParticipantRepository,
    IRoomQuestionEvaluationRepository roomQuestionEvaluationRepository,
    IRoomRepository roomRepository,
    RoomStatusUpdater roomStatusUpdater) : ISelfScopeService
{
    public Task<RoomCompleteResponse> CompleteAsync(RoomReviewCompletionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        return CompleteAsync(false, request, userId, cancellationToken);
    }

    public Task<RoomCompleteResponse> CompleteAIAsync(RoomReviewCompletionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        return CompleteAsync(true, request, userId, cancellationToken);
    }

    private Task<RoomCompleteResponse> CompleteAsync(
        bool checkAi,
        RoomReviewCompletionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return db.RunTransactionAsync<RoomCompleteResponse>(async ct =>
            {
                var isAIRoom = false;
                if (checkAi)
                {
                    var roomType = await db.Rooms.AsNoTracking()
                        .Where(e => e.Id == request.RoomId)
                        .Select(e => e.Type)
                        .FirstOrDefaultAsync(cancellationToken) ?? throw NotFoundException.Create<Room>(request.RoomId);

                    isAIRoom = checkAi && roomType == SERoomType.AI;

                    if (isAIRoom)
                    {
                        // At first start review
                        await roomStatusUpdater.StartReviewAsync(request.RoomId, false, cancellationToken);
                    }
                }

                var roomCompleteResponse = new RoomCompleteResponse { AutoClosed = false, };

                var roomParticipant = await FindParticipantWithValidateAsync(request.RoomId, userId, ct);

                var resultReview = roomParticipant.Review;

                if (isAIRoom)
                {
                    if (resultReview is null)
                    {
                        resultReview = new RoomReview(roomParticipant, SERoomReviewState.Open);
                        await db.RoomReview.AddAsync(resultReview, cancellationToken);
                    }
                }
                else
                {
                    if (resultReview is null)
                    {
                        throw new NotFoundException("The final review of the user in the room was not found");
                    }

                    if (resultReview.Review.Length < 1)
                    {
                        throw new UserException("The final review of the user in the room is not filled");
                    }
                }

                resultReview.State = SERoomReviewState.Closed;

                await roomQuestionEvaluationRepository.SubmitAsync(request.RoomId, userId, ct);

                var roomReadyClose = await roomRepository.IsReadyToCloseAsync(request.RoomId, ct);

                if (roomReadyClose)
                {
                    await roomStatusUpdater.CloseAsync(request.RoomId, false, cancellationToken);
                    roomCompleteResponse.AutoClosed = true;
                }

                await db.SaveChangesAsync(ct);

                return roomCompleteResponse;
            },
            cancellationToken);
    }

    private async Task<RoomParticipant> FindParticipantWithValidateAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var roomParticipant = await roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(roomId, userId, cancellationToken);

        if (roomParticipant is null)
        {
            throw new UserException("You are not a member of the room");
        }

        if (roomParticipant.Room.Status != SERoomStatus.Review)
        {
            throw new UserException("The room is not in the review status");
        }

        return roomParticipant;
    }
}
