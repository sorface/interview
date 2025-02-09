using System.Data;
using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomReviews.Mappers;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;
using Interview.Domain.Rooms.Service;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Rooms.RoomReviews.Services;

public class RoomReviewService(
    IRoomReviewRepository roomReviewRepository,
    IRoomRepository roomRepository,
    IRoomQuestionEvaluationRepository roomQuestionEvaluationRepository,
    IRoomMembershipChecker membershipChecker,
    AppDbContext db,
    IRoomParticipantRepository roomParticipantRepository,
    RoomStatusUpdater roomStatusUpdater)
    : IRoomReviewService
{
    public async Task<UserRoomReviewResponse?> GetUserRoomReviewAsync(UserRoomReviewRequest request, CancellationToken cancellationToken)
    {
        await membershipChecker.EnsureUserMemberOfRoomAsync(request.UserId, request.RoomId, cancellationToken);

        var review = await db.RoomReview
            .Include(e => e.Participant)
            .AsNoTracking()
            .Where(e => e.Participant.Room.Id == request.RoomId && e.Participant.User.Id == request.UserId)
            .Select(e => new { e.State, e.Id, e.Review })
            .FirstOrDefaultAsync(cancellationToken);
        return review is null
            ? null
            : new UserRoomReviewResponse { State = review.State.EnumValue, Review = review.Review, Id = review.Id };
    }

    public Task<IPagedList<RoomReviewPageDetail>> FindPageAsync(RoomReviewPageRequest request, CancellationToken cancellationToken = default)
    {
        var specification = Spec<RoomReview>.Any;
        if (request.Filter.RoomId is not null)
        {
            specification &= new Spec<RoomReview>(review => review.Participant.Id == request.Filter.RoomId);
        }

        if (request.Filter.State is not null)
        {
            var state = SERoomReviewState.FromEnum(request.Filter.State.Value);
            specification &= new Spec<RoomReview>(review => review.State == state);
        }

        return roomReviewRepository.GetDetailedPageAsync(
            specification,
            request.Page.PageNumber,
            request.Page.PageSize,
            cancellationToken);
    }

    public async Task<RoomReviewDetail> CreateAsync(RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var roomParticipant = await roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(request.RoomId, userId, cancellationToken);

        if (roomParticipant is null)
        {
            throw new UserException("You are not a member of the room");
        }

        if (roomParticipant.Room.Status != SERoomStatus.Review)
        {
            throw new UserException("The room is not in the review status");
        }

        var roomReview = new RoomReview(roomParticipant) { Review = request.Review, };

        await roomReviewRepository.CreateAsync(roomReview, cancellationToken);

        return RoomReviewDetailMapper.Instance.Map(roomReview);
    }

    public async Task<UpsertReviewResponse> UpsertAsync(RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.RunTransactionAsync<UpsertReviewResponse>(async ct =>
            {
                var roomParticipant = await FindParticipantWithValidateAsync(request.RoomId, userId, ct);

                var review = roomParticipant.Review;

                var created = false;

                if (review is null)
                {
                    review = new RoomReview(roomParticipant) { Review = request.Review };

                    await roomReviewRepository.CreateAsync(review, ct);

                    created = true;
                }
                else
                {
                    if (review.State != SERoomReviewState.Open)
                    {
                        throw new UserException("the final review is not subject to change");
                    }

                    review.Review = request.Review;

                    await roomReviewRepository.UpdateAsync(review, ct);
                }

                return RoomReviewDetailMapper.InstanceUpsert(created).Map(review);
            },
            cancellationToken);
    }

    public Task<RoomCompleteResponse> CompleteAsync(RoomReviewCompletionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        return db.RunTransactionAsync<RoomCompleteResponse>(async ct =>
            {
                var roomCompleteResponse = new RoomCompleteResponse { AutoClosed = false, };

                var roomParticipant = await FindParticipantWithValidateAsync(request.RoomId, userId, ct);

                var resultReview = roomParticipant.Review;

                if (roomParticipant.Room.Type == SERoomType.AI)
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

    public async Task<RoomReviewDetail> UpdateAsync(Guid id, RoomReviewUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var resultReview = await roomReviewRepository.FindByIdDetailedAsync(id, cancellationToken);

        if (resultReview is null)
        {
            throw NotFoundException.Create<RoomReview>(id);
        }

        if (resultReview.Participant.Room is null)
        {
            throw new UserException("The review is not linked to the room");
        }

        if (resultReview.Participant.Room.Status != SERoomStatus.Review)
        {
            throw new UserException("Room should be in Review status");
        }

        resultReview.Review = request.Review;

        var state = SERoomReviewState.FromEnum(request.State);

        if (state is null)
        {
            throw new NotFoundException($"State not found with value {request.State}");
        }

        resultReview.State = state;

        await roomReviewRepository.UpdateAsync(resultReview, cancellationToken);

        return RoomReviewDetailMapper.Instance.Map(resultReview);
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
