using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Services;
using Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestionEvaluations;
using Interview.Infrastructure.RoomQuestions;
using Interview.Infrastructure.RoomReviews;
using Interview.Infrastructure.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations.Rooms;

public class RoomReviewServiceTest
{
    [Fact]
    public async Task GetUserRoomReview_Should_Throw_Access_Denied()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, false, null);

        var request = new UserRoomReviewRequest { UserId = userId, RoomId = roomId, };

        await Assert.ThrowsAsync<AccessDeniedException>(() => service.GetUserRoomReviewAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetUserRoomReview_Should_Return_Null()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, true, null);

        var request = new UserRoomReviewRequest { UserId = userId, RoomId = roomId, };
        var result = await service.GetUserRoomReviewAsync(request, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Adding a new entity to the final review using the upsert method")]
    public async Task UpsertWithCreateNewEntity()
    {
        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());
        var (service, userId, roomId) = CreateService(memoryDatabase, true, SERoomStatus.Review);

        (await memoryDatabase.RoomReview
                .Include(roomReview => roomReview.Participant)
                .FirstOrDefaultAsync(roomReview => roomReview.Participant.RoomId == roomId && roomReview.Participant.UserId == userId, CancellationToken.None))
            .Should().BeNull();

        var roomReviewCreateRequest = new RoomReviewCreateRequest() { Review = "TEST", RoomId = roomId, };

        await service.UpsertAsync(roomReviewCreateRequest, userId, CancellationToken.None);

        var review = await memoryDatabase.RoomReview
            .Include(roomReview => roomReview.Participant)
            .FirstOrDefaultAsync(roomReview => roomReview.Participant.RoomId == roomId && roomReview.Participant.UserId == userId, CancellationToken.None);

        review.Should().NotBeNull();

        review?.Review.Should().Be(roomReviewCreateRequest.Review);
        review?.Id.Should().NotBeEmpty();
        review?.Participant.RoomId.Should().NotBeEmpty().And.Be(roomReviewCreateRequest.RoomId);
        review?.Participant.UserId.Should().NotBeEmpty();
        review?.State.Should().Be(SERoomReviewState.Open);
    }

    [Fact(DisplayName = "Changing the final review without creating a new one")]
    public async Task UpsertWithExistsEntity()
    {
        var ct = new CancellationToken();

        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());
        var (service, userId, roomId) = CreateService(memoryDatabase, true, SERoomStatus.Review);

        var user = new User(Guid.NewGuid(), "test_nickname", Guid.NewGuid().ToString());
        var room = new Domain.Rooms.Room("TEST", SERoomAccessType.Private, SERoomType.Standard);

        var roomParticipant = new RoomParticipant(user, room, SERoomParticipantType.Expert);

        var roomReview = new RoomReview(roomParticipant, SERoomReviewState.Open);

        await memoryDatabase.Users.AddAsync(user, ct);
        await memoryDatabase.Rooms.AddAsync(room, ct);
        await memoryDatabase.RoomParticipants.AddAsync(roomParticipant, ct);
        await memoryDatabase.RoomReview.AddAsync(roomReview, ct);

        await memoryDatabase.SaveChangesAsync(ct);

        (await memoryDatabase.RoomParticipants
            .Include(participant => participant.Review)
            .Where(participant => participant.Id == roomParticipant.Id)
            .Select(participant => participant.Review)
            .FirstOrDefaultAsync(ct)).Should().NotBeNull();

        var roomReviewCreateRequest = new RoomReviewCreateRequest { Review = "TEST", RoomId = roomId, };

        await service.UpsertAsync(roomReviewCreateRequest, userId, CancellationToken.None);

        var review = await memoryDatabase.RoomReview
            .Include(roomReviewItem => roomReviewItem.Participant)
            .FirstOrDefaultAsync(roomReviewItem => roomReviewItem.Participant.RoomId == roomId && roomReviewItem.Participant.UserId == userId, ct);

        review.Should().NotBeNull();

        review?.Review.Should().Be(roomReviewCreateRequest.Review);
        review?.Id.Should().NotBeEmpty().And.Be(review.Id);
        review?.Participant.RoomId.Should().NotBeEmpty().And.Be(roomReviewCreateRequest.RoomId);
        review?.Participant.UserId.Should().NotBeEmpty();
        review?.State.Should().Be(SERoomReviewState.Open);
    }

    [Fact(DisplayName = "Completion of the review with the transfer of all comments to the Submitted status and closing of the room")]
    public async Task CompletionReview()
    {
        var ct = CancellationToken.None;
        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());

        var user1 = new User(Guid.NewGuid(), "user1", Guid.NewGuid().ToString());
        var user2 = new User(Guid.NewGuid(), "user2", Guid.NewGuid().ToString());

        var question = new Question("question_test");
        var question1 = new Question("question_test_1");

        var room = new Domain.Rooms.Room("test", SERoomAccessType.Private, SERoomType.Standard) { Status = SERoomStatus.Review, };

        await memoryDatabase.Users.AddRangeAsync(user1, user2);
        await memoryDatabase.Rooms.AddAsync(room, ct);
        await memoryDatabase.Questions.AddRangeAsync(question, question1);

        var roomParticipant1 = new RoomParticipant(user1, room, SERoomParticipantType.Expert);
        var roomParticipant2 = new RoomParticipant(user2, room, SERoomParticipantType.Expert);

        await memoryDatabase.RoomParticipants.AddRangeAsync(roomParticipant1, roomParticipant2);

        var roomReview1 = new RoomReview(roomParticipant1, SERoomReviewState.Closed) { Review = "TEST_REVIEW_USER_1", };
        var roomReview2 = new RoomReview(roomParticipant2, SERoomReviewState.Open) { Review = "TEST_REVIEW_USER_2", };

        await memoryDatabase.RoomReview.AddRangeAsync(roomReview1, roomReview2);

        var roomQuestion1 = new RoomQuestion
        {
            Question = question,
            QuestionId = question.Id,
            Room = room,
            RoomId = room.Id,
            State = RoomQuestionState.Closed,
            Order = 0
        };

        var roomQuestion2 = new RoomQuestion
        {
            Question = question1,
            QuestionId = question1.Id,
            Room = room,
            RoomId = room.Id,
            State = RoomQuestionState.Closed,
            Order = 1
        };

        await memoryDatabase.RoomQuestions.AddRangeAsync(roomQuestion1, roomQuestion2);

        var roomQuestionEvaluation1 =
            new RoomQuestionEvaluation { RoomQuestionId = roomQuestion1.Id, Mark = 10, CreatedBy = user1, State = SERoomQuestionEvaluationState.Submitted };
        var roomQuestionEvaluation2 =
            new RoomQuestionEvaluation { RoomQuestionId = roomQuestion2.Id, Mark = 10, CreatedBy = user2, State = SERoomQuestionEvaluationState.Draft };

        await memoryDatabase.RoomQuestionEvaluation.AddRangeAsync(roomQuestionEvaluation1, roomQuestionEvaluation2);

        await memoryDatabase.SaveChangesAsync(ct);

        var userAccessor = new CurrentUserAccessor(user2);

        var iRoomParticipantRepository = new RoomParticipantRepository(memoryDatabase);
        var iRoomMembershipChecker = new RoomMembershipChecker(userAccessor, iRoomParticipantRepository);
        var iRoomQuestionEvaluationRepository = new RoomQuestionEvaluationRepository(memoryDatabase);
        var iRoomRepository = new RoomRepository(memoryDatabase);
        var iRoomReviewRepository = new RoomReviewRepository(memoryDatabase);

        var service = new RoomReviewService(
            iRoomReviewRepository,
            iRoomMembershipChecker,
            memoryDatabase,
            iRoomParticipantRepository,
            new RoomReviewCompleter(
                memoryDatabase,
                iRoomParticipantRepository,
                iRoomQuestionEvaluationRepository,
                iRoomRepository,
                new RoomStatusUpdater(memoryDatabase, new RoomQuestionRepository(memoryDatabase)))
        );

        var roomCompleteResponse = await service.CompleteAsync(new RoomReviewCompletionRequest { RoomId = room.Id }, user2.Id, ct);

        roomCompleteResponse.AutoClosed.Should().BeTrue();

        var expectedReview = await memoryDatabase.RoomReview.FirstOrDefaultAsync(roomReviewItem => roomReviewItem.Id == roomReview2.Id, ct);

        expectedReview?.State.Should().Be(SERoomReviewState.Closed);

        var expectedRoom = await memoryDatabase.Rooms.FirstOrDefaultAsync(roomItem => roomItem.Id == room.Id, ct);

        expectedRoom?.Status.Should().Be(SERoomStatus.Close);

        var expectedEvaluation = await memoryDatabase.RoomQuestionEvaluation
            .FirstOrDefaultAsync(roomQuestionEvaluationItem => roomQuestionEvaluationItem.Id == roomQuestionEvaluation2.Id, ct);

        expectedEvaluation?.State.Should().Be(SERoomQuestionEvaluationState.Submitted);
    }

    [Fact(DisplayName = "Adding a new entity to the final review using the upsert method")]
    public async Task FinishReview()
    {
        await using var memoryDatabase = new TestAppDbContextFactory().Create(new TestSystemClock());
        var (service, userId, roomId) = CreateService(memoryDatabase, true, SERoomStatus.Review);

        (await memoryDatabase.RoomReview
                .Include(roomReview => roomReview.Participant)
                .FirstOrDefaultAsync(roomReview => roomReview.Participant.RoomId == roomId && roomReview.Participant.UserId == userId, CancellationToken.None))
            .Should().BeNull();

        var roomReviewCreateRequest = new RoomReviewCreateRequest() { Review = "TEST", RoomId = roomId, };

        await service.UpsertAsync(roomReviewCreateRequest, userId, CancellationToken.None);

        var review = await memoryDatabase.RoomReview
            .Include(roomReview => roomReview.Participant)
            .FirstOrDefaultAsync(roomReview => roomReview.Participant.RoomId == roomId && roomReview.Participant.UserId == userId, CancellationToken.None);

        review.Should().NotBeNull();

        review?.Review.Should().Be(roomReviewCreateRequest.Review);
        review?.Id.Should().NotBeEmpty();
        review?.Participant.RoomId.Should().NotBeEmpty().And.Be(roomReviewCreateRequest.RoomId);
        review?.Participant.UserId.Should().NotBeEmpty();
        review?.State.Should().Be(SERoomReviewState.Open);
    }

    [Theory]
    [InlineData("", EVRoomReviewState.Closed)]
    [InlineData("", EVRoomReviewState.Open)]
    [InlineData("test", EVRoomReviewState.Closed)]
    [InlineData("Test", EVRoomReviewState.Open)]
    [InlineData("custom review", EVRoomReviewState.Closed)]
    [InlineData("CuStOM", EVRoomReviewState.Open)]
    public async Task GetUserRoomReview_Should_Return_Review(string review, EVRoomReviewState state)
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, true, null);
        var user = appDbContext.Users.First(e => e.Id == userId);
        var room = appDbContext.Rooms.First(e => e.Id == roomId);
        var roomParticipant = new RoomParticipant(user, room, SERoomParticipantType.Expert);
        appDbContext.RoomParticipants.AddRange(roomParticipant);
        appDbContext.RoomReview.Add(new RoomReview(roomParticipant, SERoomReviewState.FromEnum(state)!) { Review = review, });
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        var request = new UserRoomReviewRequest { UserId = userId, RoomId = roomId, };
        var result = await service.GetUserRoomReviewAsync(request, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Review.Should().Be(review);
        result.State.Should().Be(state);
    }

    private static (IRoomReviewService Service, Guid UserId, Guid RoomId) CreateService(AppDbContext db, bool addParticipant, SERoomStatus? roomStatus)
    {
        var user = new User("test user", "ID");
        db.Users.Add(user);
        var room = new Domain.Rooms.Room("MY ROOM", SERoomAccessType.Private, SERoomType.Standard) { Status = roomStatus == null ? SERoomStatus.Active : roomStatus };
        db.Rooms.Add(room);
        db.SaveChanges();

        if (addParticipant)
        {
            var roomParticipant = new RoomParticipant(user, room, SERoomParticipantType.Expert);
            db.RoomParticipants.Add(roomParticipant);
            db.SaveChanges();
        }

        db.ChangeTracker.Clear();

        var userAccessor = new CurrentUserAccessor();
        userAccessor.SetUser(user);

        var service = new RoomReviewService(
            new RoomReviewRepository(db),
            new RoomMembershipChecker(userAccessor, new RoomParticipantRepository(db)),
            db,
            new RoomParticipantRepository(db),
            new RoomReviewCompleter(
                db,
                new RoomParticipantRepository(db),
                new RoomQuestionEvaluationRepository(db),
                new RoomRepository(db),
                new RoomStatusUpdater(db, new RoomQuestionRepository(db))));

        return (service, user.Id, room.Id);
    }
}
