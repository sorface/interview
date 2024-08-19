using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Rooms.RoomReviews.Services;
using Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestionEvaluations;
using Interview.Infrastructure.RoomReviews;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Users;

namespace Interview.Test.Integrations;

public class RoomReviewServiceTest
{
    [Fact]
    public async Task GetUserRoomReview_Should_Throw_Access_Denied()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, false);

        var request = new UserRoomReviewRequest { UserId = userId, RoomId = roomId, };

        await Assert.ThrowsAsync<AccessDeniedException>(() => service.GetUserRoomReviewAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetUserRoomReview_Should_Return_Null()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var (service, userId, roomId) = CreateService(appDbContext, true);

        var request = new UserRoomReviewRequest { UserId = userId, RoomId = roomId, };
        var result = await service.GetUserRoomReviewAsync(request, CancellationToken.None);
        result.Should().BeNull();
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
        var (service, userId, roomId) = CreateService(appDbContext, true);
        var user = appDbContext.Users.First(e => e.Id == userId);
        var room = appDbContext.Rooms.First(e => e.Id == roomId);
        appDbContext.RoomReview.Add(new RoomReview(user, room, SERoomReviewState.FromEnum(state)!) { Review = review, });
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();

        var request = new UserRoomReviewRequest { UserId = userId, RoomId = roomId, };
        var result = await service.GetUserRoomReviewAsync(request, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Review.Should().Be(review);
        result.State.Should().Be(state);
    }

    private static (IRoomReviewService Service, Guid UserId, Guid RoomId) CreateService(AppDbContext db, bool addParticipant)
    {
        var user = new User("test user", "ID");
        db.Users.Add(user);
        var room = new Room("MY ROOM", SERoomAccessType.Private);
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
            new UserRepository(db),
            new RoomRepository(db),
            new RoomQuestionEvaluationRepository(db),
            new RoomMembershipChecker(userAccessor, new RoomParticipantRepository(db)),
            db);

        return (service, user.Id, room.Id);
    }
}
