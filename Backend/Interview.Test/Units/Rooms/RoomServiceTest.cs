using Interview.Domain;
using Interview.Domain.Events;
using Interview.Domain.Events.Storage;
using Interview.Domain.Questions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Tags;
using Interview.Domain.Users;
using Interview.Infrastructure.RoomInvites;
using Moq;

namespace Interview.Test.Units.Rooms;

public class RoomServiceTest
{
    private readonly Mock<IRoomRepository> _roomRepository;

    private readonly RoomService _roomService;

    public RoomServiceTest()
    {
        _roomRepository = new Mock<IRoomRepository>();
        var questionRepository = new Mock<IQuestionRepository>();
        var roomQuestionRepository = new Mock<IRoomQuestionRepository>();
        var userRepository = new Mock<IUserRepository>();
        var eventDispatcher = new Mock<IRoomEventDispatcher>();
        var roomQuestionReactionRepository = new Mock<IRoomQuestionReactionRepository>();
        var tagRepository = new Mock<ITagRepository>();
        var roomStateRepository = new Mock<IRoomStateRepository>();
        var roomInviteRepository = new Mock<IRoomInviteRepository>();

        _roomService = new RoomService(
            _roomRepository.Object,
            roomQuestionRepository.Object,
            questionRepository.Object,
            userRepository.Object,
            eventDispatcher.Object,
            roomQuestionReactionRepository.Object,
            tagRepository.Object,
            new Mock<IRoomParticipantRepository>().Object,
            new Mock<IAppEventRepository>().Object,
            roomStateRepository.Object,
            new EmptyEventStorage(),
            roomInviteRepository.Object,
            new CurrentUserAccessor());
    }

    [Fact(DisplayName = "Patch update of room when request name is null")]
    public async void PatchUpdateRoomWhenRequestNameIsNull()
    {
        var roomPatchUpdateRequest = new RoomUpdateRequest { Name = null };

        await Assert.ThrowsAsync<UserException>(() =>
            _roomService.UpdateAsync(Guid.NewGuid(), roomPatchUpdateRequest));
    }

    [Fact(DisplayName = "Patch update of room when room not found")]
    public async Task PatchUpdateRoomWhenRoomNotFound()
    {
        var roomPatchUpdateRequest = new RoomUpdateRequest { Name = "new_value_name_room", TwitchChannel = "TwitchCH" };
        var roomId = Guid.NewGuid();

        _roomRepository.Setup(repository => repository.FindByIdAsync(roomId, default))
            .ReturnsAsync((Room?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _roomService.UpdateAsync(roomId, roomPatchUpdateRequest));
    }
}
