using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomReviews.Mappers;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Rooms.RoomReviews.Services;

public class RoomReviewService : IRoomReviewService
{
    private readonly IRoomReviewRepository _roomReviewRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoomQuestionEvaluationRepository _roomQuestionEvaluationRepository;
    private readonly IRoomMembershipChecker _membershipChecker;
    private readonly AppDbContext _db;

    public RoomReviewService(
        IRoomReviewRepository roomReviewRepository,
        IUserRepository userRepository,
        IRoomRepository roomRepository,
        IRoomQuestionEvaluationRepository roomQuestionEvaluationRepository,
        IRoomMembershipChecker membershipChecker,
        AppDbContext db)
    {
        _roomReviewRepository = roomReviewRepository;
        _userRepository = userRepository;
        _roomRepository = roomRepository;
        _roomQuestionEvaluationRepository = roomQuestionEvaluationRepository;
        _membershipChecker = membershipChecker;
        _db = db;
    }

    public async Task<UserRoomReviewResponse?> GetUserRoomReviewAsync(UserRoomReviewRequest request, CancellationToken cancellationToken)
    {
        await _membershipChecker.EnsureUserMemberOfRoomAsync(request.UserId, request.RoomId, cancellationToken);
        var review = await _db.RoomReview
            .Include(e => e.Room)
            .Include(e => e.User)
            .AsNoTracking()
            .Where(e => e.Room!.Id == request.RoomId && e.User!.Id == request.UserId)
            .Select(e => new { State = e.SeRoomReviewState, Id = e.Id, Review = e.Review, })
            .FirstOrDefaultAsync(cancellationToken);
        return review is null
            ? null
            : new UserRoomReviewResponse { State = review.State.EnumValue, Review = review.Review, Id = review.Id };
    }

    public Task<IPagedList<RoomReviewPageDetail>> FindPageAsync(
        RoomReviewPageRequest request,
        CancellationToken cancellationToken = default)
    {
        var specification = Spec<RoomReview>.Any;
        if (request.Filter.RoomId is not null)
        {
            specification &= new Spec<RoomReview>(review => review.Room!.Id == request.Filter.RoomId);
        }

        if (request.Filter.State is not null)
        {
            var state = SERoomReviewState.FromEnum(request.Filter.State.Value);
            specification &= new Spec<RoomReview>(review => review.SeRoomReviewState == state);
        }

        return _roomReviewRepository.GetDetailedPageAsync(
            specification,
            request.Page.PageNumber,
            request.Page.PageSize,
            cancellationToken);
    }

    public async Task<RoomReviewDetail> CreateAsync(
        RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<User>(userId);
        }

        var room = await _roomRepository.FindByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        if (room.Status != SERoomStatus.Review)
        {
            throw new UserException("Room should be in Review status");
        }

        var roomReview = new RoomReview(user, room, SERoomReviewState.Open) { Review = request.Review, };

        await _roomReviewRepository.CreateAsync(roomReview, cancellationToken);
        await _roomQuestionEvaluationRepository.SubmitAsync(room.Id, user.Id, cancellationToken);

        return RoomReviewDetailMapper.Instance.Map(roomReview);
    }

    public async Task<RoomReviewDetail> UpdateAsync(
        Guid id, RoomReviewUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var roomReview = await _roomReviewRepository.FindByIdDetailedAsync(id, cancellationToken);

        if (roomReview is null)
        {
            throw NotFoundException.Create<RoomReview>(id);
        }

        if (roomReview.Room is null)
        {
            throw new UserException("The review is not linked to the room");
        }

        if (roomReview.Room.Status != SERoomStatus.Review)
        {
            throw new UserException("Room should be in Review status");
        }

        roomReview.Review = request.Review;

        var state = SERoomReviewState.FromEnum(request.State);

        if (state is null)
        {
            throw new NotFoundException($"State not found with value {request.State}");
        }

        roomReview.SeRoomReviewState = state;

        await _roomReviewRepository.UpdateAsync(roomReview, cancellationToken);

        return RoomReviewDetailMapper.Instance.Map(roomReview);
    }
}
