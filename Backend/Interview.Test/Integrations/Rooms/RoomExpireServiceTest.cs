using FluentAssertions;
using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomExpireServices;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Moq;

namespace Interview.Test.Integrations.Rooms;

public class RoomExpireServiceTest
{
    private readonly AppDbContext _dbContext;
    private readonly RoomExpireSettings _roomExpireSettings;
    private readonly Mock<ISystemClock> _mockClock;
    private readonly Mock<ILogger<RoomExpireService>> _mockLogger;
    private readonly RoomExpireService _service;

    public static IEnumerable<object[]> TestData
    {
        get
        {
            foreach (var roomAccessType in SERoomAccessType.List)
            {
                foreach (var roomType in SERoomType.List)
                {
                    yield return [roomAccessType, roomType];
                }
            }
        }
    }

    public RoomExpireServiceTest()
    {
        _mockClock = new Mock<ISystemClock>();
        _dbContext = new TestAppDbContextFactory().Create(_mockClock.Object);

        _roomExpireSettings = new RoomExpireSettings { ReviewDayExpiration = 30, ActiveDayExpiration = 60 };
        _mockLogger = new Mock<ILogger<RoomExpireService>>();

        _service = new RoomExpireService(_dbContext, _roomExpireSettings, _mockClock.Object, _mockLogger.Object)
        {
            PageSize = 10,
        };
    }

    [Theory(DisplayName = "Should process expired review rooms correctly")]
    [MemberData(nameof(TestData))]
    public async Task ProcessAsync_ShouldMarkReviewRoomsAsExpired(SERoomAccessType roomAccessType, SERoomType roomType)
    {
        var now = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var reviewRoom = new Room("test", roomAccessType, roomType) { Status = SERoomStatus.Review, UpdateDate = now.AddDays(-31) };
        await _dbContext.Rooms.AddAsync(reviewRoom);
        var newRoom = await _dbContext.Rooms.AddAsync(new Room("test 2", roomAccessType, roomType) { Status = SERoomStatus.New, UpdateDate = now.AddDays(-31) });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        await _service.ProcessAsync(CancellationToken.None);

        reviewRoom = _dbContext.Rooms.Single(e => e.Id == reviewRoom.Id);
        reviewRoom.Status.Should().Be(SERoomStatus.Expire);
        var actualNewRoom = await _dbContext.Rooms.FindAsync(newRoom.Entity.Id);
        actualNewRoom.Status.Should().Be(SERoomStatus.New);
    }

    [Theory(DisplayName = "Should process expired active rooms correctly")]
    [MemberData(nameof(TestData))]
    public async Task ProcessAsync_ShouldMarkActiveRoomsAsExpired(SERoomAccessType roomAccessType, SERoomType roomType)
    {
        var now = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var activeRoom = new Room("test", roomAccessType, roomType) { Status = SERoomStatus.Active, UpdateDate = now.AddDays(-61) };
        await _dbContext.Rooms.AddAsync(activeRoom);
        var newRoom = await _dbContext.Rooms.AddAsync(new Room("test 2", roomAccessType, roomType) { Status = SERoomStatus.New, UpdateDate = now.AddDays(-31) });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        await _service.ProcessAsync(CancellationToken.None);

        activeRoom = _dbContext.Rooms.Single(e => e.Id == activeRoom.Id);
        activeRoom.Status.Should().Be(SERoomStatus.Expire);
        var actualNewRoom = await _dbContext.Rooms.FindAsync(newRoom.Entity.Id);
        actualNewRoom.Status.Should().Be(SERoomStatus.New);
    }

    [Theory(DisplayName = "Should not process active rooms with a date that does not match the overdue date")]
    [MemberData(nameof(TestData))]
    public async Task ProcessAsync_ShouldNotMarkActiveRoomsAsExpired(SERoomAccessType roomAccessType, SERoomType roomType)
    {
        var now = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var activeRoom = new Room("test", roomAccessType, roomType) { Status = SERoomStatus.Active, UpdateDate = now.AddDays(-1) };
        await _dbContext.Rooms.AddAsync(activeRoom);
        var newRoom = await _dbContext.Rooms.AddAsync(new Room("test 2", roomAccessType, roomType) { Status = SERoomStatus.New, UpdateDate = now.AddDays(-31) });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        await _service.ProcessAsync(CancellationToken.None);

        activeRoom = _dbContext.Rooms.Single(e => e.Id == activeRoom.Id);
        activeRoom.Status.Should().NotBe(SERoomStatus.Expire);
        var actualNewRoom = await _dbContext.Rooms.FindAsync(newRoom.Entity.Id);
        actualNewRoom.Status.Should().Be(SERoomStatus.New);
    }

    [Fact(DisplayName = "Should handle empty database correctly")]
    public async Task ProcessAsync_ShouldNotFailOnEmptyDatabase()
    {
        var now = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        await _service.ProcessAsync(CancellationToken.None);

        _mockLogger.Verify(l => l.Log(
            It.Is<LogLevel>(lvl => lvl == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Should throw exception on DbContext failure")]
    public async Task ProcessAsync_ShouldLogErrorOnDbContextFailure()
    {
        await _dbContext.DisposeAsync();

        Func<Task> act = async () => await _service.ProcessAsync(CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        _mockLogger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Theory(DisplayName = "Should not modify rooms with other statuses")]
    [MemberData(nameof(TestData))]
    public async Task ProcessAsync_ShouldNotChangeStatusForOtherRooms(SERoomAccessType roomAccessType, SERoomType roomType)
    {
        var now = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var pendingRoom = new Room("test", roomAccessType, roomType) { Status = SERoomStatus.New, UpdateDate = now.AddDays(-61) };
        await _dbContext.Rooms.AddAsync(pendingRoom);
        await _dbContext.Rooms.AddAsync(new Room("test 2", roomAccessType, roomType) { Status = SERoomStatus.New, UpdateDate = now.AddDays(-31) });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        await _service.ProcessAsync(CancellationToken.None);

        var rooms = _dbContext.Rooms.ToList();
        rooms.Should().AllSatisfy(e => e.Status.Should().Be(SERoomStatus.New));
    }

    [Theory(DisplayName = "Should process rooms in batches")]
    [MemberData(nameof(TestData))]
    public async Task ProcessAsync_ShouldProcessRoomsInBatches(SERoomAccessType roomAccessType, SERoomType roomType)
    {
        var now = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var rooms = new List<Room>();
        for (int i = 0; i < 20; i++)
        {
            rooms.Add(new Room("test", roomAccessType, roomType) { Status = SERoomStatus.Review, UpdateDate = now.AddDays(-31) });
        }

        await _dbContext.Rooms.AddRangeAsync(rooms);
        var newRoom = await _dbContext.Rooms.AddAsync(new Room("test 2", roomAccessType, roomType) { Status = SERoomStatus.New, UpdateDate = now.AddDays(-31) });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        await _service.ProcessAsync(CancellationToken.None);

        var roomIds = rooms.ConvertAll(e => e.Id);
        rooms = _dbContext.Rooms.Where(e => roomIds.Contains(e.Id)).ToList();
        rooms.All(r => r.Status == SERoomStatus.Expire).Should().BeTrue();
        var actualNewRoom = await _dbContext.Rooms.FindAsync(newRoom.Entity.Id);
        actualNewRoom.Status.Should().Be(SERoomStatus.New);
    }

    [Theory(DisplayName = "Should not process rooms with a date that does not match the overdue date.")]
    [MemberData(nameof(TestData))]
    public async Task ProcessAsync_ShouldNotProcess_NonExpiredRoomsInBatches(SERoomAccessType roomAccessType, SERoomType roomType)
    {
        var now = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockClock.Setup(c => c.UtcNow).Returns(now);

        var rooms = new List<Room>();
        for (int i = 0; i < 20; i++)
        {
            rooms.Add(new Room("test", roomAccessType, roomType) { Status = SERoomStatus.Review, UpdateDate = now.AddDays(-1) });
        }

        await _dbContext.Rooms.AddRangeAsync(rooms);
        var newRoom = await _dbContext.Rooms.AddAsync(new Room("test 2", roomAccessType, roomType) { Status = SERoomStatus.New, UpdateDate = now.AddDays(-31) });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        await _service.ProcessAsync(CancellationToken.None);

        var roomIds = rooms.ConvertAll(e => e.Id);
        rooms = _dbContext.Rooms.Where(e => roomIds.Contains(e.Id)).ToList();
        rooms.All(r => r.Status != SERoomStatus.Expire).Should().BeTrue();
        var actualNewRoom = await _dbContext.Rooms.FindAsync(newRoom.Entity.Id);
        actualNewRoom.Status.Should().Be(SERoomStatus.New);
    }
}
