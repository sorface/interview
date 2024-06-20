using System.Collections.ObjectModel;
using FluentAssertions;
using Interview.Backend.WebSocket.Events.ConnectionListener;
using Interview.Domain.Connections;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.Events;
using Interview.Domain.Questions;
using Interview.Domain.Repository;
using Moq;

namespace Interview.Test.Units.Processors;

public class QuestionPostProcessorTest
{
    private readonly Mock<IRoomEventDispatcher> _roomEventDispatcher;
    private readonly Mock<IActiveRoomSource> _roomConnectionListener;
    private readonly QuestionPostProcessor _questionPostProcessor;

    public QuestionPostProcessorTest()
    {
        _roomEventDispatcher = new Mock<IRoomEventDispatcher>();
        _roomConnectionListener = new Mock<IActiveRoomSource>();

        _questionPostProcessor = new QuestionPostProcessor(
            _roomEventDispatcher.Object,
            _roomConnectionListener.Object
        );
    }

    [Fact(DisplayName = "Sending an event successfully if there are active rooms")]
    public async Task SendingEventSuccessfullyWhenActiveRoomsPresent()
    {
        var activeRoomId = new Guid("0560200c-dbbe-4646-bbf4-b9898fcbfc7d");
        var activeRooms = new List<Guid> { activeRoomId }.AsReadOnly();

        var questionId = Guid.Parse("b47e5f7f-6371-452d-a736-c5c3f0e17ab0");

        var originalQuestion = new Question("value") { Id = questionId };
        var currentQuestion = new Question("value2") { Id = questionId };

        var modifyEntities = new List<(Entity Original, Entity Current)> { (originalQuestion, currentQuestion) };

        var cancellationToken = new CancellationToken(false);

        _roomConnectionListener.Setup(roomConnectionListener => roomConnectionListener.ActiveRooms)
            .Returns(activeRooms);
        _roomEventDispatcher.Setup(dispatcher =>
            dispatcher.WriteAsync(It.IsAny<IRoomEvent>(), It.IsAny<CancellationToken>()));

        await _questionPostProcessor.ProcessModifiedAsync(modifyEntities, cancellationToken);

        var eventArgumentCaptor = new ArgumentCaptor<QuestionChangeEvent>();

        _roomEventDispatcher.Verify(
            mock => mock.WriteAsync(eventArgumentCaptor.Capture(), cancellationToken),
            Times.Once
        );

        var questionChangeEvent = eventArgumentCaptor.Value;

        questionChangeEvent.Stateful.Should().BeFalse();
        questionChangeEvent.RoomId.Should().Be(activeRoomId);
        questionChangeEvent.Type.Should().Be(EventType.ChangeQuestion);

        var payload = questionChangeEvent.Value;

        payload.Should().NotBeNull();

        payload?.QuestionId.Should().Be(questionId);
        payload?.OldValue.Should().Be(originalQuestion.Value);
        payload?.NewValue.Should().Be(currentQuestion.Value);
    }

    [Fact(DisplayName = "The event was not sent because there are no active rooms")]
    public async Task EventNotSentWhenActiveRoomsNotPresent()
    {
        var questionId = Guid.Parse("b47e5f7f-6371-452d-a736-c5c3f0e17ab0");

        var originalQuestion = new Question("value") { Id = questionId };
        var currentQuestion = new Question("value2") { Id = questionId };

        var modifyEntities = new List<(Entity Original, Entity Current)> { (originalQuestion, currentQuestion) };

        var cancellationToken = new CancellationToken(false);

        _roomConnectionListener.Setup(roomConnectionListener => roomConnectionListener.ActiveRooms)
            .Returns(new Collection<Guid>());

        await _questionPostProcessor.ProcessModifiedAsync(modifyEntities, cancellationToken);

        var eventArgumentCaptor = new ArgumentCaptor<QuestionChangeEvent>();

        _roomEventDispatcher.Verify(
            mock => mock.WriteAsync(eventArgumentCaptor.Capture(), cancellationToken),
            Times.Never
        );
    }
}
