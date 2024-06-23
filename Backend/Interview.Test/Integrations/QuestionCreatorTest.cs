using Bogus;
using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Categories;
using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Questions.Services;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Interview.Infrastructure.Questions;
using Interview.Infrastructure.RoomQuestions;
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
            var category = new Category { Name = "Test" };
            await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());

            appDbContext.Categories.Add(category);
            await appDbContext.SaveChangesAsync();
            appDbContext.ChangeTracker.Clear();

            if (room is not null)
            {
                appDbContext.Rooms.Add(room);
                await appDbContext.SaveChangesAsync();
                appDbContext.ChangeTracker.Clear();
            }

            var roomMemberChecker = new Mock<IRoomMembershipChecker>();
            roomMemberChecker
                .Setup(e => e.EnsureCurrentUserMemberOfRoomAsync(room == null ? It.IsAny<Guid>() : room.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var creator = CreateQuestionCreate(appDbContext, roomMemberChecker.Object);
            var questionCreateRequest = new QuestionCreateRequest
            {
                Tags = new HashSet<Guid>(),
                Value = value,
                Type = EVQuestionType.Private,
                CategoryId = category.Id,
            };
            var question = await creator.CreateAsync(questionCreateRequest, room?.Id, CancellationToken.None);
            question.Should().NotBeNull().And.Match<QuestionItem>(e => e.Value == value);
            roomMemberChecker.Verify(e => e.EnsureCurrentUserMemberOfRoomAsync(room == null ? It.IsAny<Guid>() : room.Id, It.IsAny<CancellationToken>()), room is not null ? Times.Once() : Times.Never());
            question.Category.Should().NotBeNull();
            question.Category!.Id.Should().Be(category.Id);
        }

        [Fact(DisplayName = "Creation should not succeed if the room is not available")]
        public async Task CreateShouldFailIfRoomIsNotAvailable()
        {
            await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
            var room = new Room("Test Room", SERoomAccessType.Public);
            appDbContext.Rooms.Add(room);
            var category = new Category { Name = "Test" };
            appDbContext.Categories.Add(category);
            await appDbContext.SaveChangesAsync();
            appDbContext.ChangeTracker.Clear();

            var roomMemberChecker = new Mock<IRoomMembershipChecker>();
            roomMemberChecker
                .Setup(e => e.EnsureCurrentUserMemberOfRoomAsync(room.Id, It.IsAny<CancellationToken>()))
                .Throws<UnavailableException>();

            var creator = CreateQuestionCreate(appDbContext, roomMemberChecker.Object);
            var questionCreateRequest = new QuestionCreateRequest
            {
                Tags = new HashSet<Guid>(),
                Value = "Test",
                Type = EVQuestionType.Private,
                CategoryId = category.Id,
            };
            await Assert.ThrowsAsync<UnavailableException>(() => creator.CreateAsync(questionCreateRequest, room.Id, CancellationToken.None));
            roomMemberChecker.Verify(e => e.EnsureCurrentUserMemberOfRoomAsync(room.Id, It.IsAny<CancellationToken>()), Times.Once());
        }

        private static IQuestionService CreateQuestionCreate(AppDbContext appDbContext, IRoomMembershipChecker roomMembershipChecker)
        {
            var questionRepository = new QuestionRepository(appDbContext);
            var tagRepository = new TagRepository(appDbContext);
            var currentUser = new CurrentUserAccessor();
            currentUser.SetUser(appDbContext.Users.First());
            return new QuestionService(
                questionRepository,
                new QuestionNonArchiveRepository(appDbContext),
                new ArchiveService<Question>(appDbContext),
                tagRepository,
                roomMembershipChecker,
                currentUser,
                appDbContext);
        }

        private class UnavailableException : Exception
        {
        }
    }
}
