using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Events.Storage;
using Interview.Domain.Questions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.BusinessAnalytic;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.RoomQuestions;
using Interview.Infrastructure.Rooms;
using Interview.Infrastructure.Users;
using Microsoft.Extensions.Logging.Abstractions;

namespace Interview.Test.Integrations.Rooms;

public class RoomServiceBusinessAnalyticTest
{
    [Fact]
    public async Task Should_Throw_UserException_When_StartDate_After_EndDate()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var question = new Question("test");
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        // Arrange
        var request = new BusinessAnalyticRequest
        {
            Filter = new BusinessAnalyticRequestFilter
            {
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 5, 1)
            },
            DateSort = EVSortOrder.Asc
        };

        var roomService = CreateRoomService(appDbContext, user);

        // Act
        var act = async () => await roomService.GetBusinessAnalyticAsync(request);

        // Assert
        await act.Should().ThrowAsync<UserException>()
            .WithMessage("The start date must be before the end date.");
    }

    [Fact]
    public async Task Should_Throw_UserException_When_DateRange_Exceeds_TwoMonths()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var question = new Question("test");
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        // Arrange
        var request = new BusinessAnalyticRequest
        {
            Filter = new BusinessAnalyticRequestFilter
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 4, 1)
            },
            DateSort = EVSortOrder.Asc
        };

        var roomService = CreateRoomService(appDbContext, user);

        // Act
        var act = async () => await roomService.GetBusinessAnalyticAsync(request);

        // Assert
        await act.Should().ThrowAsync<UserException>()
            .WithMessage("The end date must not exceed the maximum allowable value. (not older than 2 months)");
    }

    [Fact]
    public async Task Should_Return_Correct_Analytic_Data_For_AI_Rooms()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var question = new Question("test");
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        // Arrange
        var now = new DateTime(2024, 1, 15);
        var rooms = new List<Domain.Rooms.Room>
        {
            new("test", SERoomAccessType.Public, SERoomType.AI) { Id = Guid.NewGuid(), CreateDate = new DateTime(2024, 1, 1), Status = SERoomStatus.Active },
            new("test", SERoomAccessType.Public, SERoomType.AI) { Id = Guid.NewGuid(), CreateDate = new DateTime(2024, 1, 1), Status = SERoomStatus.New },
            new("test", SERoomAccessType.Private, SERoomType.AI) { Id = Guid.NewGuid(), CreateDate = new DateTime(2024, 1, 2), Status = SERoomStatus.Active },
            new("test", SERoomAccessType.Public, SERoomType.Standard) { Id = Guid.NewGuid(), CreateDate = new DateTime(2024, 1, 1), Status = SERoomStatus.Active },
        };

        appDbContext.Rooms.AddRange(rooms);
        await appDbContext.SaveChangesAsync();

        var request = new BusinessAnalyticRequest
        {
            Filter = new BusinessAnalyticRequestFilter
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 31),
                RoomTypes = [EVRoomType.AI]
            },
            DateSort = EVSortOrder.Asc
        };

        var roomService = CreateRoomService(appDbContext, user);

        // Act
        var result = await roomService.GetBusinessAnalyticAsync(request);

        // Assert
        result.Standard.Should().BeEmpty();
        result.Ai.Should().HaveCount(2);
        result.Ai[0].Status.Should().HaveCount(2);
        result.Ai[0].Status.Should().ContainSingle(e => e.Name == EVRoomStatus.Active && e.Count == 1);
        result.Ai[0].Status.Should().ContainSingle(e => e.Name == EVRoomStatus.New && e.Count == 1);
        result.Ai[1].Status.Should().HaveCount(1);
        result.Ai[1].Status.Should().ContainSingle(e => e.Name == EVRoomStatus.Active && e.Count == 1);
    }

    [Fact]
    public async Task Should_Filter_By_AccessType()
    {
        var testSystemClock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(testSystemClock);
        var user = new User("test", "test");
        appDbContext.Users.Add(user);
        var question = new Question("test");
        appDbContext.Questions.Add(question);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();

        // Arrange
        var rooms = new List<Domain.Rooms.Room>
        {
            new("test", SERoomAccessType.Public, SERoomType.AI) { Id = Guid.NewGuid(), CreateDate = new DateTime(2024, 1, 1), Status = SERoomStatus.Active },
            new("test", SERoomAccessType.Private, SERoomType.AI) { Id = Guid.NewGuid(), CreateDate = new DateTime(2024, 1, 1), Status = SERoomStatus.Active },
        };

        appDbContext.Rooms.AddRange(rooms);
        await appDbContext.SaveChangesAsync();

        var request = new BusinessAnalyticRequest
        {
            Filter = new BusinessAnalyticRequestFilter
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 31),
                AccessType = EVRoomAccessType.Public
            },
            DateSort = EVSortOrder.Asc
        };

        var roomService = CreateRoomService(appDbContext, user);

        // Act
        var result = await roomService.GetBusinessAnalyticAsync(request);

        // Assert
        result.Ai.Should().HaveCount(1);
        result.Ai[0].Status.Should().ContainSingle(s => s.Name == EVRoomStatus.Active && s.Count == 1);
    }

    private static RoomService CreateRoomService(AppDbContext appDbContext, User? user = null)
    {
        var userAccessor = new CurrentUserAccessor();
        if (user is not null)
        {
            userAccessor.SetUser(user);
        }

        var time = new TestSystemClock { UtcNow = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), };

        var roomParticipantService = new RoomParticipantService(
            new RoomParticipantRepository(appDbContext),
            new RoomRepository(appDbContext),
            new UserRepository(appDbContext),
            userAccessor,
            new PermissionRepository(appDbContext));

        return new RoomService(
            new EmptyRoomEventDispatcher(),
            new EmptyHotEventStorage(),
            new RoomInviteService(appDbContext, roomParticipantService, NullLogger<RoomInviteService>.Instance),
            userAccessor,
            roomParticipantService,
            appDbContext,
            new NullLogger<RoomService>(),
            time,
            new RoomAnalyticService(appDbContext),
            new RoomStatusUpdater(appDbContext, new RoomQuestionRepository(appDbContext)));
    }
}
