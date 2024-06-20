using FluentAssertions;
using Interview.Domain.Events.DatabaseProcessors;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomTimers;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Moq;

namespace Interview.Test.Units.Processors;

public class RoomActiveStatusPreProcessorTest
{
    private readonly RoomActiveStatusPreProcessor _roomActiveStatusPreProcessor;

    public RoomActiveStatusPreProcessorTest()
    {
        var databaseContext = new TestAppDbContextFactory().Create(new SystemClock());
        var mockLogger = new Mock<ILogger<RoomActiveStatusPreProcessor>>();
        _roomActiveStatusPreProcessor = new RoomActiveStatusPreProcessor(mockLogger.Object, databaseContext);
    }

    [Fact(DisplayName = "The room has been changed to the active status. The current date is entered in the timer.")]
    public async Task RoomStatusChangeWithUpdateActiveStartTime()
    {
        var roomOriginal = new Room("default", SERoomAccessType.Private) { Status = SERoomStatus.New };
        var roomCurrent = new Room("default", SERoomAccessType.Private)
        {
            Timer = new RoomTimer(),
            Status = SERoomStatus.Active
        };

        var entities = new List<(Entity Original, Entity Current)> { (roomOriginal, roomCurrent) };

        await _roomActiveStatusPreProcessor.ProcessModifiedAsync(entities, new CancellationToken());

        roomCurrent.Timer.Should().NotBeNull();
        roomCurrent.Timer.ActualStartTime.Should().NotBeNull();
    }

    [Fact(DisplayName =
        "The room has not been changed to the active status. The current date is entered in the timer.")]
    public async Task RoomStatusNotChangeWithUpdateActiveStartTime()
    {
        var roomOriginal = new Room("default", SERoomAccessType.Private) { Status = SERoomStatus.New };
        var roomCurrent = new Room("default", SERoomAccessType.Private)
        {
            Timer = new RoomTimer(),
            Status = SERoomStatus.Review
        };

        var entities = new List<(Entity Original, Entity Current)> { (roomOriginal, roomCurrent) };

        await _roomActiveStatusPreProcessor.ProcessModifiedAsync(entities, new CancellationToken());

        roomCurrent.Timer.Should().NotBeNull();
        roomCurrent.Timer.ActualStartTime.Should().BeNull();
    }
}
