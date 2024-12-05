using FluentAssertions;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Users;
using Moq;

namespace Interview.Test.Units.Processors;

public class RoomConfigurationPostProcessorTest
{
    private readonly Mock<IRoomEventDispatcher> _dispatcher;
    private readonly RoomConfigurationPostProcessor _roomConfigurationPostProcessor;

    public RoomConfigurationPostProcessorTest()
    {
        _dispatcher = new Mock<IRoomEventDispatcher>();
        var user = new Mock<ICurrentUserAccessor>();
        user.Setup(e => e.UserId).Returns(Guid.Parse("1D9958C2-7081-4B77-AE27-7F879A80CECA"));
        _roomConfigurationPostProcessor = new RoomConfigurationPostProcessor(_dispatcher.Object, user.Object);
    }

    [Fact(DisplayName = "Sending an event about adding a room configuration")]
    public async Task SendEventWhenCreateRoomConfiguration()
    {
        _dispatcher.Setup(dispatcher =>
            dispatcher.WriteAsync(It.IsAny<RoomCodeEditorChangeEvent>(), new CancellationToken()));

        var roomConfiguration = new RoomConfiguration
        {
            Id = Guid.NewGuid(),
            CodeEditorContent = "test",
            CodeEditorChangeSource = EVRoomCodeEditorChangeSource.User
        };
        await _roomConfigurationPostProcessor.ProcessAddedAsync(roomConfiguration, new CancellationToken());

        var argumentCaptor = new ArgumentCaptor<RoomCodeEditorChangeEvent>();

        _dispatcher.Verify(dispatcher =>
            dispatcher.WriteAsync(argumentCaptor.Capture(), It.IsAny<CancellationToken>()));

        var @event = argumentCaptor.Value;

        @event.RoomId.Should().Be(roomConfiguration.Id);
        @event.Value!.Content.Should().Be(roomConfiguration.CodeEditorContent);
    }

    [Fact(DisplayName = "Sending an event about changing the configuration of the room.")]
    public async Task SendEventWhenUpdateRoomConfiguration()
    {
        _dispatcher.Setup(dispatcher =>
            dispatcher.WriteAsync(It.IsAny<RoomCodeEditorChangeEvent>(), new CancellationToken()));

        var roomConfigurationOrigin = new RoomConfiguration
        {
            Id = Guid.NewGuid(),
            CodeEditorContent = "test",
            CodeEditorChangeSource = EVRoomCodeEditorChangeSource.User,
        };
        var roomConfigurationCurrent = new RoomConfiguration
        {
            Id = Guid.NewGuid(),
            CodeEditorContent = "tests",
            CodeEditorChangeSource = EVRoomCodeEditorChangeSource.User
        };
        await _roomConfigurationPostProcessor.ProcessModifiedAsync(
            roomConfigurationOrigin,
            roomConfigurationCurrent,
            new CancellationToken());

        var argumentCaptor = new ArgumentCaptor<RoomCodeEditorChangeEvent>();

        _dispatcher.Verify(dispatcher =>
            dispatcher.WriteAsync(argumentCaptor.Capture(), It.IsAny<CancellationToken>()));

        var @event = argumentCaptor.Value;

        @event.RoomId.Should().Be(roomConfigurationCurrent.Id);
        @event.Value!.Content.Should().Be(roomConfigurationCurrent.CodeEditorContent);
    }
}
