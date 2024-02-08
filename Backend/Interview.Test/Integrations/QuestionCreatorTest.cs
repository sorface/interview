using Bogus;
using FluentAssertions;
using Interview.Domain.Questions;
using Interview.Domain.Questions.Services;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Interview.Infrastructure.Database;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.Tags;
using Moq;

namespace Interview.Test.Integrations
{
    public class QuestionCreatorTest
    {
        public static IEnumerable<object?[]> CreateData
        {
            get
            {
                var faker = new Faker();
                var roomFaker = new FakerFactory().Room();
                foreach (var _ in Enumerable.Range(1, 15))
                {
                    var room = faker.Random.Bool() ? null : roomFaker.Generate();
                    yield return new object?[] { faker.Random.Word(), room, };
                }
            }
        }

        [Theory]
        [MemberData(nameof(CreateData))]
        public async Task Create(string value, Room? room)
        {
            await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
            if (room is not null)
            {
                appDbContext.Rooms.Add(room);
                await appDbContext.SaveChangesAsync();
                appDbContext.ChangeTracker.Clear();
            }

            var roomMemberChecker = new Mock<IRoomMembershipChecker>();
            roomMemberChecker
                .Setup(e => e.EnsureCurrentUserMemberOfRoomAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var creator = CreateQuestionCreate(appDbContext, roomMemberChecker.Object);
            var questionCreateRequest = new QuestionCreateRequest { Tags = new HashSet<Guid>(), Value = value };
            var question = await creator.CreateAsync(questionCreateRequest, room?.Id, CancellationToken.None);
            question.Should().NotBeNull().And.Match<Question>(e => e.Value == value);
        }

        [Fact(DisplayName = "Creation should not succeed if the room is not available")]
        public async Task CreateShouldFailIfRoomIsNotAvailable()
        {
            await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
            var room = new Room("Test Room", string.Empty);
            appDbContext.Rooms.Add(room);
            await appDbContext.SaveChangesAsync();
            appDbContext.ChangeTracker.Clear();

            var roomMemberChecker = new Mock<IRoomMembershipChecker>();
            roomMemberChecker
                .Setup(e => e.EnsureCurrentUserMemberOfRoomAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Throws<UnavailableException>();

            var creator = CreateQuestionCreate(appDbContext, roomMemberChecker.Object);
            var questionCreateRequest = new QuestionCreateRequest { Tags = new HashSet<Guid>(), Value = "Test" };
            await Assert.ThrowsAsync<UnavailableException>(() => creator.CreateAsync(questionCreateRequest, room.Id, CancellationToken.None));
        }

        private static QuestionCreator CreateQuestionCreate(AppDbContext appDbContext, IRoomMembershipChecker roomMembershipChecker)
        {
            var questionRepository = new QuestionRepository(appDbContext);
            var tagRepository = new TagRepository(appDbContext);
            var currentUser = new CurrentUserAccessor();
            currentUser.SetUser(appDbContext.Users.First());
            return new QuestionCreator(tagRepository, questionRepository, roomMembershipChecker);
        }

        private class UnavailableException : Exception
        {
        }
    }
}
