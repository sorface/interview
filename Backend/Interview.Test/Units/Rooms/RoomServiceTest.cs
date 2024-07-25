using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.Storage;
using Interview.Domain.Questions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Tags;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Serilog.Core;

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
        var roomInviteRepository = new Mock<IRoomInviteService>();
        var participantService = new Mock<IRoomParticipantService>();

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
            new CurrentUserAccessor(),
            participantService.Object,
            new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().Options),
            NullLogger<RoomService>.Instance
        );
    }

    [Fact(DisplayName = "Patch update of room when request name is null")]
    public async void PatchUpdateRoomWhenRequestNameIsNull()
    {
        var roomPatchUpdateRequest = new RoomUpdateRequest
        {
            Name = null,
            Questions = new List<RoomQuestionRequest>(),
        };

        await Assert.ThrowsAsync<UserException>(() =>
            _roomService.UpdateAsync(Guid.NewGuid(), roomPatchUpdateRequest));
    }
}
